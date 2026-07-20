using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.Submission.Application.Commands.PublishSubmission;
using RegOS.Submission.Application.Queries.GetSubmissionSnapshot;
using RegOS.Submission.Application.Validation;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;
using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using RegOS.Submission.Application.Tests.Fixtures;

namespace RegOS.Submission.Application.Tests;

// Integration tests — the read-side snapshot query against the real dev Postgres
// (docker postgres-local). Publishes to create the snapshot, then projects it.
public sealed class GetSubmissionSnapshotTests : IAsyncLifetime
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

    private async Task<(SubmissionId Id, List<Guid> Versions)> SeedDraftAsync(int count)
    {
        await using var ctx = New();
        var (applicationId, productId) = await TestApplications.EnsureAsync(ctx);

        var submission = SubmissionAggregate.Create(
            applicationId, SeededSubmissionType, "Snapshot Query Sub " + Guid.NewGuid());

        var versions = new List<Guid>();
        for (var i = 0; i < count; i++)
        {
            var doc = ProductDocumentAggregate.Create(
                productId, SeededCer, "Snapshot Query Doc " + Guid.NewGuid());
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
            versions.Add(doc.CurrentVersionId!.Value.Value);
        }

        ctx.Submissions.Add(submission);
        await ctx.SaveChangesAsync();
        _submissionIds.Add(submission.Id.Value);
        return (submission.Id, versions);
    }

    private async Task<SubmissionId> PublishedSubmissionAsync(int documentCount)
    {
        var (submissionId, _) = await SeedDraftAsync(documentCount);
        await using var ctx = New();
        var handler = new PublishSubmissionHandler(
            new SubmissionValidator(new SubmissionRepository(ctx), ctx),
            new SubmissionRepository(ctx),
            new SubmissionSnapshotRepository(ctx));
        await handler.HandleAsync(new PublishSubmissionCommand(submissionId), default);
        return submissionId;
    }

    private static GetSubmissionSnapshotHandler QueryFor(RegOSDbContext ctx) =>
        new(ctx);

    // --- Tests ---------------------------------------------------------------

    [Fact]
    public async Task Query_ReturnsPublishedDossier()
    {
        var submissionId = await PublishedSubmissionAsync(2);

        await using var ctx = New();
        var dossier = await QueryFor(ctx).HandleAsync(submissionId, default);

        dossier.Should().NotBeNull();
        dossier!.SubmissionId.Should().Be(submissionId.Value);
        dossier.PublishedAt.Should().NotBeNull();
        dossier.Documents.Should().HaveCount(2);
    }

    [Fact]
    public async Task Query_ReturnsDocumentsOrderedByDisplayOrder()
    {
        var (submissionId, versions) = await SeedDraftAsync(3);
        await using (var ctx = New())
        {
            await new PublishSubmissionHandler(
                new SubmissionValidator(new SubmissionRepository(ctx), ctx),
                new SubmissionRepository(ctx),
                new SubmissionSnapshotRepository(ctx))
                .HandleAsync(new PublishSubmissionCommand(submissionId), default);
        }

        await using var query = New();
        var dossier = await QueryFor(query).HandleAsync(submissionId, default);

        dossier!.Documents.Select(d => d.DisplayOrder).Should().Equal(1, 2, 3);
        // Same versions, in published order.
        dossier.Documents.Select(d => d.DocumentVersionId).Should().Equal(versions);
    }

    [Fact]
    public async Task Query_WhenNotPublished_ReturnsNull()
    {
        // A draft submission has no snapshot.
        var (submissionId, _) = await SeedDraftAsync(1);

        await using var ctx = New();
        var dossier = await QueryFor(ctx).HandleAsync(submissionId, default);

        dossier.Should().BeNull();
    }

    [Fact]
    public async Task Query_PublishedAtMatchesSubmission()
    {
        var submissionId = await PublishedSubmissionAsync(1);

        await using var ctx = New();
        var storedPublishedAt = await ctx.Submissions.AsNoTracking()
            .Where(s => s.Id == submissionId)
            .Select(s => s.PublishedAt)
            .FirstAsync();

        var dossier = await QueryFor(ctx).HandleAsync(submissionId, default);

        dossier!.PublishedAt.Should().Be(storedPublishedAt);
    }

    [Fact]
    public async Task Query_PreservesDocumentCount()
    {
        var submissionId = await PublishedSubmissionAsync(7);

        await using var ctx = New();
        var dossier = await QueryFor(ctx).HandleAsync(submissionId, default);

        dossier!.Documents.Should().HaveCount(7);
    }
}
