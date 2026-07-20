using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Commands.PublishSubmission;
using RegOS.Submission.Application.Validation;
using RegOS.Submission.Domain.Snapshot;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;
using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Application.Tests.Fixtures;

namespace RegOS.Submission.Application.Tests;

// Integration tests — the publish workflow now also captures an immutable snapshot,
// atomically, against the real dev Postgres (docker postgres-local).
public sealed class PublishSubmissionSnapshotTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private static readonly DocumentTypeId SeededCer =
        new(Guid.Parse("50000000-0000-0000-0000-000000000001"));
    private static readonly SubmissionTypeId SeededSubmissionType =
        new(Guid.Parse("40000000-0000-0000-0000-000000000001"));

    private readonly List<Guid> _submissionIds = [];
    private readonly List<Guid> _documentIds = [];

    private static DbContextOptions<RegOSDbContext> Options() =>
        new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

    private static RegOSDbContext New() => new(Options());

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var ctx = New();

        if (_submissionIds.Count > 0)
        {
            var subIds = _submissionIds.ToArray();
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"SubmissionSnapshotDocuments\" WHERE \"SubmissionSnapshotId\" IN " +
                "(SELECT \"Id\" FROM \"SubmissionSnapshots\" WHERE \"SubmissionId\" = ANY({0}))",
                new object[] { subIds });
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"SubmissionSnapshots\" WHERE \"SubmissionId\" = ANY({0})",
                new object[] { subIds });
        }

        foreach (var id in _submissionIds)
        {
            var sub = await ctx.Submissions
                .Include(s => s.Documents)
                .FirstOrDefaultAsync(s => s.Id == new SubmissionId(id));
            if (sub is not null)
                ctx.Submissions.Remove(sub);
        }
        await ctx.SaveChangesAsync();

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

    // --- Seeding -------------------------------------------------------------

    // Seeds a draft submission with `count` active documents attached in order,
    // returning the submission id and the pinned versions in display order.
    private async Task<(SubmissionId Id, List<DocumentVersionId> Versions)>
        SeedSubmissionAsync(int count)
    {
        await using var ctx = New();
        var (applicationId, productId) = await TestApplications.EnsureAsync(ctx);

        var submission = SubmissionAggregate.Create(
            applicationId, SeededSubmissionType, "Publish-Snapshot Sub " + Guid.NewGuid());

        var versions = new List<DocumentVersionId>();
        for (var i = 0; i < count; i++)
        {
            var doc = ProductDocumentAggregate.Create(
                productId, SeededCer, "Publish-Snapshot Doc " + Guid.NewGuid());
            doc.AddInitialVersion(
                originalFileName: "cer.pdf",
                storedFileName: "v1.pdf",
                contentType: "application/pdf",
                fileSize: 1024,
                storagePath: $"products/{productId.Value}/{doc.Id.Value}/v1.pdf",
                checksum: "sha256-x");
            doc.Activate();
            ctx.ProductDocuments.Add(doc);
            _documentIds.Add(doc.Id.Value);

            submission.AttachDocument(doc.Id, doc.CurrentVersionId!.Value);
            versions.Add(doc.CurrentVersionId!.Value);
        }

        ctx.Submissions.Add(submission);
        await ctx.SaveChangesAsync();
        _submissionIds.Add(submission.Id.Value);
        return (submission.Id, versions);
    }

    private static PublishSubmissionHandler PublishHandlerFor(RegOSDbContext ctx) =>
        new(
            new SubmissionValidator(new SubmissionRepository(ctx), ctx),
            new SubmissionRepository(ctx),
            new SubmissionSnapshotRepository(ctx));

    // --- Test 1: publishing creates a snapshot -------------------------------

    [Fact]
    public async Task Publish_CreatesSnapshotWithAllVersions()
    {
        var (submissionId, versions) = await SeedSubmissionAsync(2);

        await using (var ctx = New())
        {
            var result = await PublishHandlerFor(ctx).HandleAsync(
                new PublishSubmissionCommand(submissionId), default);
            result.Published.Should().BeTrue();
        }

        await using (var ctx = New())
        {
            var submission = await ctx.Submissions.AsNoTracking()
                .FirstAsync(s => s.Id == submissionId);
            submission.Status.Should().Be(SubmissionStatus.Published);
            submission.PublishedAt.Should().NotBeNull();

            var snapshot = await new SubmissionSnapshotRepository(ctx)
                .GetBySubmissionIdAsync(submissionId, default);
            snapshot.Should().NotBeNull();
            snapshot!.Documents.Select(d => d.DocumentVersionId)
                .Should().BeEquivalentTo(versions);
        }
    }

    // --- Test 2: snapshot preserves document order ---------------------------

    [Fact]
    public async Task Publish_SnapshotPreservesDocumentOrder()
    {
        var (submissionId, versions) = await SeedSubmissionAsync(3);

        await using (var ctx = New())
        {
            await PublishHandlerFor(ctx).HandleAsync(
                new PublishSubmissionCommand(submissionId), default);
        }

        await using (var ctx = New())
        {
            var snapshot = await new SubmissionSnapshotRepository(ctx)
                .GetBySubmissionIdAsync(submissionId, default);

            snapshot!.Documents
                .OrderBy(d => d.DisplayOrder)
                .Select(d => d.DisplayOrder)
                .Should().Equal(1, 2, 3);

            // Same versions, same order as published.
            snapshot.Documents
                .OrderBy(d => d.DisplayOrder)
                .Select(d => d.DocumentVersionId)
                .Should().Equal(versions);
        }
    }

    // --- Test 3: snapshot references the exact published versions -------------

    [Fact]
    public async Task Publish_SnapshotReferencesPublishedVersions()
    {
        var (submissionId, versions) = await SeedSubmissionAsync(2);

        await using (var ctx = New())
        {
            await PublishHandlerFor(ctx).HandleAsync(
                new PublishSubmissionCommand(submissionId), default);
        }

        await using (var ctx = New())
        {
            var snapshot = await new SubmissionSnapshotRepository(ctx)
                .GetBySubmissionIdAsync(submissionId, default);

            snapshot!.Documents.Select(d => d.DocumentVersionId)
                .Should().BeEquivalentTo(versions);
        }
    }

    // --- Test 4: only one snapshot -------------------------------------------

    [Fact]
    public async Task Publish_Twice_LeavesExactlyOneSnapshot()
    {
        var (submissionId, _) = await SeedSubmissionAsync(1);

        await using (var ctx = New())
        {
            var first = await PublishHandlerFor(ctx).HandleAsync(
                new PublishSubmissionCommand(submissionId), default);
            first.Published.Should().BeTrue();
        }

        await using (var ctx = New())
        {
            // Already published — rejected as a lifecycle conflict before any
            // snapshot work happens. The assertion below is the point: the
            // failed republish must leave no second snapshot behind.
            var call = () => PublishHandlerFor(ctx).HandleAsync(
                new PublishSubmissionCommand(submissionId), default);

            await call.Should().ThrowAsync<BusinessRuleViolationException>();
        }

        await using (var ctx = New())
        {
            var count = await ctx.SubmissionSnapshots
                .CountAsync(s => s.SubmissionId == submissionId);
            count.Should().Be(1);
        }
    }

    // --- Test 5: atomic rollback ---------------------------------------------

    [Fact]
    public async Task Publish_WhenSnapshotPersistenceFails_RollsBackPublish()
    {
        var (submissionId, versions) = await SeedSubmissionAsync(1);

        // Pre-seed a snapshot for this submission so the publish's own snapshot
        // insert violates the unique SubmissionId index and SaveChanges fails.
        await using (var ctx = New())
        {
            var blocker = SubmissionSnapshot.Create(
                submissionId, versions.Select((v, i) => (v, i + 1)));
            await new SubmissionSnapshotRepository(ctx).AddAsync(blocker, default);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = New())
        {
            var publish = () => PublishHandlerFor(ctx).HandleAsync(
                new PublishSubmissionCommand(submissionId), default);

            await publish.Should().ThrowAsync<DbUpdateException>();
        }

        // The publish rolled back: the submission is still Draft, and there is
        // still only the one pre-seeded snapshot.
        await using (var ctx = New())
        {
            var submission = await ctx.Submissions.AsNoTracking()
                .FirstAsync(s => s.Id == submissionId);
            submission.Status.Should().Be(SubmissionStatus.Draft);
            submission.PublishedAt.Should().BeNull();

            var count = await ctx.SubmissionSnapshots
                .CountAsync(s => s.SubmissionId == submissionId);
            count.Should().Be(1);
        }
    }
}
