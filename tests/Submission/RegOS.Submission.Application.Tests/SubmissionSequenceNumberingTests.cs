using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Primitives;
using RegOS.Submission.Application.Commands.PublishSubmission;
using RegOS.Submission.Application.Services;
using RegOS.Submission.Application.Tests.Fixtures;
using RegOS.Submission.Application.Validation;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;
using RegOS.Submission.Infrastructure.Services;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;
using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// Sequence numbering end to end, against the real Postgres — the numbering
/// policy, the aggregate's contiguity rule and the unique index only tell the
/// truth together (ADR-044).
/// </summary>
/// <remarks>
/// Its own fixture application: a sequence number is scoped to an application,
/// so a test class that shared one with another class would be sharing a
/// numbering space and contending on the index for reasons unrelated to what it
/// asserts.
/// </remarks>
public sealed class SubmissionSequenceNumberingTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private const string Fixture = "TEST-SEQUENCE-NUMBERING";

    private static readonly DocumentTypeId SeededCer =
        new(Guid.Parse("50000000-0000-0000-0000-000000000001"));
    private static readonly SubmissionTypeId SeededSubmissionType =
        new(Guid.Parse("40000000-0000-0000-0000-000000000001"));

    private readonly List<Guid> _submissionIds = [];
    private readonly List<Guid> _documentIds = [];
    private readonly ITestOutputHelper _output;

    public SubmissionSequenceNumberingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static RegOSDbContext New() =>
        new(new DbContextOptionsBuilder<RegOSDbContext>()
                .UseNpgsql(ConnectionString).Options,
            TestTenant.Context);

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var ctx = New();

        if (_submissionIds.Count > 0)
        {
            var subIds = _submissionIds.ToArray();
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"SubmissionSnapshotDocuments\" WHERE \"SubmissionSnapshotId\" IN "
                + "(SELECT \"Id\" FROM \"SubmissionSnapshots\" WHERE \"SubmissionId\" = ANY({0}))",
                new object[] { subIds });
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"SubmissionSnapshots\" WHERE \"SubmissionId\" = ANY({0})",
                new object[] { subIds });
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"SubmissionDocuments\" WHERE \"SubmissionId\" = ANY({0})",
                new object[] { subIds });
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"Submissions\" WHERE \"Id\" = ANY({0})",
                new object[] { subIds });
        }

        foreach (var id in _documentIds)
        {
            var doc = await ctx.ProductDocuments
                .Include(d => d.Versions)
                .FirstOrDefaultAsync(d => d.Id == new ProductDocumentId(id));
            if (doc is not null)
                ctx.ProductDocuments.Remove(doc);
        }

        await ctx.SaveChangesAsync();
    }

    // --- Numbering -----------------------------------------------------------

    [Fact]
    public async Task TheFirstSequenceInAnApplication_Is0000()
    {
        var submissionId = await SeedPublishableAsync();

        await PublishAsync(submissionId);

        (await SequenceOfAsync(submissionId)).Should().Be(0);
    }

    [Fact]
    public async Task EachPublish_TakesTheNextNumber()
    {
        var first = await SeedPublishableAsync();
        var second = await SeedPublishableAsync();
        var third = await SeedPublishableAsync();

        await PublishAsync(first);
        await PublishAsync(second);
        await PublishAsync(third);

        (await SequenceOfAsync(first)).Should().Be(0);
        (await SequenceOfAsync(second)).Should().Be(1);
        (await SequenceOfAsync(third)).Should().Be(2);
    }

    /// <summary>
    /// A draft is not merely unnumbered in the UI — it holds no number at all,
    /// which is what makes "null means never transmitted" a fact rather than a
    /// display convention.
    /// </summary>
    [Fact]
    public async Task ADraft_HoldsNoNumber_AndDoesNotConsumeOne()
    {
        var abandoned = await SeedPublishableAsync();
        var published = await SeedPublishableAsync();

        await PublishAsync(published);

        (await SequenceOfAsync(abandoned)).Should().BeNull();
        (await SequenceOfAsync(published)).Should().Be(0);
    }

    [Fact]
    public async Task TheNumberingPolicy_ReportsTheNextNumberAndWhatItFollows()
    {
        var submissionId = await SeedPublishableAsync();
        RegulatoryApplicationId appId;

        await using (var ctx = New())
        {
            (appId, _) = await TestApplications.EnsureAsync(ctx, Fixture);

            var before = await new SubmissionNumberingPolicy(ctx)
                .GetNextPublishSequenceNumberAsync(appId, default);

            before.Should().Be(new NextSequence(0, null));
        }

        await PublishAsync(submissionId);

        await using (var ctx = New())
        {
            var after = await new SubmissionNumberingPolicy(ctx)
                .GetNextPublishSequenceNumberAsync(appId, default);

            after.Should().Be(new NextSequence(1, 0));
        }
    }

    // --- The concurrency hypothesis (S001) -----------------------------------

    /// <summary>
    /// Simultaneous publishes against one application, at rising contention.
    /// </summary>
    /// <remarks>
    /// <b>What this asserts is the invariant, not the success rate.</b> Every
    /// publish that succeeds holds a distinct number, and the successful numbers
    /// form an unbroken run from 0000 — no duplicates, and no gaps that a later
    /// sequence would silently re-base against. That holds at every level.
    /// <para>
    /// Publishes that lose the race raise <see cref="SequenceNumberTakenException"/>
    /// (409, "try again") rather than being retried in process. The rate is
    /// <b>reported, not asserted</b> — it is evidence for the epic's register,
    /// and pinning a number here would make the test brittle for no gain.
    /// </para>
    /// <para>
    /// Two at once in one application is the realistic pathological case for a
    /// regulatory filing; twenty is not a workload but a way to make the index
    /// prove itself. Both are here because the interesting question was where
    /// between them the design stops being adequate — and the answer turned out
    /// to be <b>two</b>, which is the whole finding.
    /// </para>
    /// <para>
    /// <b>A hundred was measured and is not committed.</b> It gave 16–18%
    /// through, consistent with the trend, but a hundred concurrent DbContexts
    /// exhaust a local Postgres's <c>max_connections</c> while the rest of the
    /// suite is running — the fixture runs out before the design does, which
    /// makes it a flaky test rather than a stronger one. The numbers live in
    /// the epic's S001 note.
    /// </para>
    /// </remarks>
    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(20)]
    public async Task ConcurrentPublishes_YieldDistinctContiguousNumbers(int attempts)
    {
        var submissionIds = new List<SubmissionId>();
        for (var i = 0; i < attempts; i++)
            submissionIds.Add(await SeedPublishableAsync());

        var outcomes = await Task.WhenAll(submissionIds.Select(async id =>
        {
            try
            {
                await PublishAsync(id);
                return true;
            }
            catch (SequenceNumberTakenException)
            {
                return false;
            }
        }));

        var published = outcomes.Count(x => x);
        published.Should().BeGreaterThan(0);

        // Evidence for the epic register, not an assertion: how much of a
        // pile-up on one application actually gets through.
        _output.WriteLine(
            $"contention {attempts}: {published} published, "
            + $"{attempts - published} told to retry "
            + $"({published * 100 / attempts}% through).");

        await using var ctx = New();
        var (appId, _) = await TestApplications.EnsureAsync(ctx, Fixture);

        var numbers = await ctx.Submissions
            .AsNoTracking()
            .Where(x => x.ApplicationId == appId && x.SequenceNumber != null)
            .Select(x => x.SequenceNumber!.Value)
            .ToListAsync();

        numbers.Should().HaveCount(published);
        numbers.Should().OnlyHaveUniqueItems(
            "the unique index is the authority on uniqueness, whatever the "
            + "numbering policy read");
        numbers.Order().Should().Equal(
            Enumerable.Range(0, published),
            "successful publishes must leave an unbroken run from 0000 — a gap "
            + "would give a later sequence the wrong diff base");
    }

    // --- Helpers -------------------------------------------------------------

    private static PublishSubmissionHandler HandlerFor(RegOSDbContext ctx) =>
        new(
            new SubmissionValidator(new SubmissionRepository(ctx), ctx),
            new SubmissionNumberingPolicy(ctx),
            new SubmissionRepository(ctx),
            new SubmissionSnapshotRepository(ctx));

    private async Task PublishAsync(SubmissionId submissionId)
    {
        await using var ctx = New();

        var result = await HandlerFor(ctx)
            .HandleAsync(new PublishSubmissionCommand(submissionId), default);

        result.Published.Should().BeTrue();
    }

    private static async Task<int?> SequenceOfAsync(SubmissionId submissionId)
    {
        await using var ctx = New();

        return await ctx.Submissions
            .AsNoTracking()
            .Where(x => x.Id == submissionId)
            .Select(x => x.SequenceNumber)
            .SingleAsync();
    }

    /// <summary>A draft with one active document — enough to pass validation.</summary>
    private async Task<SubmissionId> SeedPublishableAsync()
    {
        await using var ctx = New();

        var (appId, globalProductId) =
            await TestApplications.EnsureAsync(ctx, Fixture);

        var doc = ProductDocumentAggregate.Create(
            TestTenant.Id, globalProductId, SeededCer, "Sequence Doc " + Guid.NewGuid());

        doc.AddInitialVersion(
            originalFileName: "cer.pdf",
            storedFileName: "v1.pdf",
            contentType: "application/pdf",
            fileSize: 1024,
            storagePath: $"products/{globalProductId.Value}/{doc.Id.Value}/v1.pdf",
            checksum: "sha256-x");
        doc.Activate();

        ctx.ProductDocuments.Add(doc);
        await ctx.SaveChangesAsync();
        _documentIds.Add(doc.Id.Value);

        var submission = SubmissionAggregate.Create(
            TestTenant.Id, appId, SeededSubmissionType,
            "Sequence Sub " + Guid.NewGuid());

        submission.AttachDocument(doc.Id, doc.CurrentVersionId!.Value);

        ctx.Submissions.Add(submission);
        await ctx.SaveChangesAsync();
        _submissionIds.Add(submission.Id.Value);

        return submission.Id;
    }
}
