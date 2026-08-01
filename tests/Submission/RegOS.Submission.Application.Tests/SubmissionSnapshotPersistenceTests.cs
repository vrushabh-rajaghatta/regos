using RegOS.SharedKernel.Primitives;
using System.Data.Common;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Snapshot;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;
using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using RegOS.Submission.Application.Tests.Fixtures;

namespace RegOS.Submission.Application.Tests;

// Integration tests — persist and reload SubmissionSnapshot against the real dev
// Postgres (docker postgres-local). No publish integration yet: the snapshot is
// built directly from a seeded submission's documents.
public sealed class SubmissionSnapshotPersistenceTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private static readonly DocumentTypeId SeededCer =
        new(Guid.Parse("50000000-0000-0000-0000-000000000001"));
    private static readonly SubmissionTypeId SeededSubmissionType =
        new(Guid.Parse("40000000-0000-0000-0000-000000000001"));

    private readonly List<Guid> _snapshotIds = [];
    private readonly List<Guid> _submissionIds = [];
    private readonly List<Guid> _documentIds = [];

    private static DbContextOptions<RegOSDbContext> Options() =>
        new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

    private static RegOSDbContext New() =>
        new(Options(), TestTenant.Context);

    public Task InitializeAsync() => Task.CompletedTask;

    // Delete in FK order: snapshot documents, snapshots, then submissions (which
    // cascade their own documents), then product documents. Every snapshot FK is
    // RESTRICT, so nothing is removed implicitly.
    public async Task DisposeAsync()
    {
        await using var ctx = New();

        if (_snapshotIds.Count > 0)
        {
            var ids = _snapshotIds.ToArray();
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"SubmissionSnapshotDocuments\" WHERE \"SubmissionSnapshotId\" = ANY({0})",
                new object[] { ids });
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"SubmissionSnapshots\" WHERE \"Id\" = ANY({0})",
                new object[] { ids });
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

    private async Task<SubmissionId> SeedSubmissionWithDocumentsAsync(int documentCount)
    {
        await using var ctx = New();

        var (applicationId, globalProductId) = await TestApplications.EnsureAsync(ctx);

        var submission = SubmissionAggregate.Create(TestTenant.Id, 
            applicationId, SeededSubmissionType, "Snapshot Sub " + Guid.NewGuid());

        for (var i = 0; i < documentCount; i++)
        {
            var doc = ProductDocumentAggregate.Create(TestTenant.Id, 
                globalProductId, SeededCer, "Snapshot Doc " + Guid.NewGuid());
            doc.AddInitialVersion(
                originalFileName: "cer.pdf",
                storedFileName: "v1.pdf",
                contentType: "application/pdf",
                fileSize: 1024,
                storagePath: $"products/{globalProductId.Value}/{doc.Id.Value}/v1.pdf",
                checksum: "sha256-x");
            doc.Activate();
            ctx.ProductDocuments.Add(doc);
            _documentIds.Add(doc.Id.Value);

            submission.AttachDocument(doc.Id, doc.CurrentVersionId!.Value);
        }

        ctx.Submissions.Add(submission);
        await ctx.SaveChangesAsync();
        _submissionIds.Add(submission.Id.Value);
        return submission.Id;
    }

    private async Task<SubmissionSnapshot> SnapshotFromSubmissionAsync(
        SubmissionId submissionId)
    {
        await using var ctx = New();
        var submission = await new SubmissionRepository(ctx)
            .GetByIdAsync(submissionId, default);

        return SubmissionSnapshot.Create(TestTenant.Id, 
            submissionId,
            submission!.Documents
                .OrderBy(d => d.DisplayOrder)
                .Select(d => (d.DocumentVersionId, d.DisplayOrder)));
    }

    // --- Round-trip ----------------------------------------------------------

    [Fact]
    public async Task Persist_And_ReloadById_IncludesDocumentsInOrder()
    {
        var submissionId = await SeedSubmissionWithDocumentsAsync(2);
        var snapshot = await SnapshotFromSubmissionAsync(submissionId);
        var expectedVersions = snapshot.Documents
            .OrderBy(d => d.DisplayOrder)
            .Select(d => d.DocumentVersionId)
            .ToList();
        _snapshotIds.Add(snapshot.Id.Value);

        // AddAsync stages only — the caller owns the commit.
        await using (var ctx = New())
        {
            await new SubmissionSnapshotRepository(ctx).AddAsync(snapshot, default);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = New())
        {
            var reloaded = await new SubmissionSnapshotRepository(ctx)
                .GetByIdAsync(snapshot.Id, default);

            reloaded.Should().NotBeNull();
            reloaded!.SubmissionId.Should().Be(submissionId);
            reloaded.Documents.Should().HaveCount(2);
            reloaded.Documents
                .OrderBy(d => d.DisplayOrder)
                .Select(d => d.DocumentVersionId)
                .Should().Equal(expectedVersions);
        }
    }

    [Fact]
    public async Task Persist_And_ReloadBySubmissionId_IncludesDocuments()
    {
        var submissionId = await SeedSubmissionWithDocumentsAsync(1);
        var snapshot = await SnapshotFromSubmissionAsync(submissionId);
        _snapshotIds.Add(snapshot.Id.Value);

        await using (var ctx = New())
        {
            await new SubmissionSnapshotRepository(ctx).AddAsync(snapshot, default);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = New())
        {
            var reloaded = await new SubmissionSnapshotRepository(ctx)
                .GetBySubmissionIdAsync(submissionId, default);

            reloaded.Should().NotBeNull();
            reloaded!.Id.Should().Be(snapshot.Id);
            reloaded.Documents.Should().ContainSingle();
        }
    }

    [Fact]
    public async Task SecondSnapshotForSameSubmission_ViolatesUniqueIndex()
    {
        var submissionId = await SeedSubmissionWithDocumentsAsync(1);

        var first = await SnapshotFromSubmissionAsync(submissionId);
        _snapshotIds.Add(first.Id.Value);
        await using (var ctx = New())
        {
            await new SubmissionSnapshotRepository(ctx).AddAsync(first, default);
            await ctx.SaveChangesAsync();
        }

        // A second snapshot for the same submission must be rejected by the DB.
        var second = await SnapshotFromSubmissionAsync(submissionId);
        _snapshotIds.Add(second.Id.Value);

        await using var act = New();
        await new SubmissionSnapshotRepository(act).AddAsync(second, default);

        var save = () => act.SaveChangesAsync();
        await save.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DuplicateDisplayOrderWithinSnapshot_ViolatesUniqueIndex()
    {
        var submissionId = await SeedSubmissionWithDocumentsAsync(1);
        var snapshot = await SnapshotFromSubmissionAsync(submissionId);
        _snapshotIds.Add(snapshot.Id.Value);
        await using (var ctx = New())
        {
            await new SubmissionSnapshotRepository(ctx).AddAsync(snapshot, default);
            await ctx.SaveChangesAsync();
        }

        var existing = snapshot.Documents.Single();

        // Bypass the domain (which forbids duplicates) and insert a second row with
        // the same (SubmissionSnapshotId, DisplayOrder). The unique index must reject it.
        await using var act = New();
        var insert = () => act.Database.ExecuteSqlRawAsync(
            "INSERT INTO \"SubmissionSnapshotDocuments\" " +
            "(\"Id\", \"DocumentVersionId\", \"DisplayOrder\", \"SubmissionSnapshotId\") " +
            "VALUES ({0}, {1}, {2}, {3})",
            Guid.NewGuid(),
            existing.DocumentVersionId.Value,
            existing.DisplayOrder,
            snapshot.Id.Value);

        await insert.Should().ThrowAsync<DbException>();
    }
}
