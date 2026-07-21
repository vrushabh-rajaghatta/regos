using RegOS.SharedKernel.Primitives;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Commands.AttachProductDocument;
using RegOS.Submission.Application.Commands.PublishSubmission;
using RegOS.Submission.Application.Validation;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;
using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Application.Tests.Fixtures;

namespace RegOS.Submission.Application.Tests;

// Integration tests — exercise the publish handler end-to-end against the real dev
// Postgres (docker postgres-local). Publishing is where validation, the aggregate
// invariant, and persistence come together.
public sealed class PublishSubmissionTests : IAsyncLifetime
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

        // Publishing now creates a snapshot per submission. Remove snapshots first
        // (RESTRICT FKs to both the submission and the versions) before the rest.
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

    // --- Seeding helpers -----------------------------------------------------

    private static async Task<(RegulatoryApplicationId AppId, ProductId ProductId)>
        FirstApplicationAsync(RegOSDbContext ctx)
    {
        return await TestApplications.EnsureAsync(ctx);
    }

    private async Task<ProductDocumentAggregate> SeedActiveDocumentAsync(
        RegOSDbContext ctx, ProductId productId)
    {
        var doc = ProductDocumentAggregate.Create(TenantId.New(), 
            productId, SeededCer, "Publish Doc " + Guid.NewGuid());

        doc.AddInitialVersion(
            originalFileName: "cer.pdf",
            storedFileName: "v1.pdf",
            contentType: "application/pdf",
            fileSize: 1024,
            storagePath: $"products/{productId.Value}/{doc.Id.Value}/v1.pdf",
            checksum: "sha256-x");
        doc.Activate();

        ctx.ProductDocuments.Add(doc);
        await ctx.SaveChangesAsync();
        _documentIds.Add(doc.Id.Value);
        return doc;
    }

    private async Task<SubmissionId> SeedSubmissionAsync(
        RegOSDbContext ctx, RegulatoryApplicationId appId,
        ProductDocumentAggregate? document)
    {
        var sub = SubmissionAggregate.Create(TenantId.New(), 
            appId, SeededSubmissionType, "Publish Sub " + Guid.NewGuid());

        if (document is not null)
            sub.AttachDocument(document.Id, document.CurrentVersionId!.Value);

        ctx.Submissions.Add(sub);
        await ctx.SaveChangesAsync();
        _submissionIds.Add(sub.Id.Value);
        return sub.Id;
    }

    private static PublishSubmissionHandler PublishHandlerFor(RegOSDbContext ctx) =>
        new(
            new SubmissionValidator(new SubmissionRepository(ctx), ctx),
            new SubmissionRepository(ctx),
            new SubmissionSnapshotRepository(ctx));

    // --- Publish: validation gate --------------------------------------------

    [Fact]
    public async Task Publish_InvalidSubmission_DoesNotPublishAndReturnsIssues()
    {
        SubmissionId submissionId;
        await using (var ctx = New())
        {
            var (appId, _) = await FirstApplicationAsync(ctx);
            // No documents attached — not ready.
            submissionId = await SeedSubmissionAsync(ctx, appId, document: null);
        }

        PublishSubmissionResult result;
        await using (var act = New())
        {
            result = await PublishHandlerFor(act).HandleAsync(
                new PublishSubmissionCommand(submissionId), default);
        }

        result.Published.Should().BeFalse();
        result.Validation.Should().NotBeNull();
        result.Validation!.IsValid.Should().BeFalse();

        // And it is still a Draft in the database.
        await using (var ctx = New())
        {
            var reloaded = await ctx.Submissions.AsNoTracking()
                .FirstAsync(s => s.Id == submissionId);
            reloaded.Status.Should().Be(SubmissionStatus.Draft);
        }
    }

    [Fact]
    public async Task Publish_ValidSubmission_PublishesAndPersists()
    {
        SubmissionId submissionId;
        await using (var ctx = New())
        {
            var (appId, productId) = await FirstApplicationAsync(ctx);
            var doc = await SeedActiveDocumentAsync(ctx, productId);
            submissionId = await SeedSubmissionAsync(ctx, appId, doc);
        }

        PublishSubmissionResult result;
        await using (var act = New())
        {
            result = await PublishHandlerFor(act).HandleAsync(
                new PublishSubmissionCommand(submissionId), default);
        }

        result.Published.Should().BeTrue();
        result.Validation.Should().BeNull();

        await using (var ctx = New())
        {
            var reloaded = await ctx.Submissions.AsNoTracking()
                .FirstAsync(s => s.Id == submissionId);
            reloaded.Status.Should().Be(SubmissionStatus.Published);
        }
    }

    [Fact]
    public async Task Publish_AlreadyPublished_ThrowsBusinessRuleViolation()
    {
        SubmissionId submissionId;
        await using (var ctx = New())
        {
            var (appId, productId) = await FirstApplicationAsync(ctx);
            var doc = await SeedActiveDocumentAsync(ctx, productId);
            submissionId = await SeedSubmissionAsync(ctx, appId, doc);
        }

        // First publish succeeds.
        await using (var ctx = New())
        {
            await PublishHandlerFor(ctx).HandleAsync(
                new PublishSubmissionCommand(submissionId), default);
        }

        // Republishing is a lifecycle conflict, not an unmet readiness
        // criterion: there is no checklist the caller could work through, so it
        // raises rather than returning issues. (ADR-009)
        await using var act = New();

        var call = () => PublishHandlerFor(act).HandleAsync(
            new PublishSubmissionCommand(submissionId), default);

        await call.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.SubmissionNotDraft);
    }

    // --- Publish: immutability -----------------------------------------------

    [Fact]
    public async Task Publish_ThenAttach_IsRejected()
    {
        SubmissionId submissionId;
        ProductDocumentId secondDocId;
        await using (var ctx = New())
        {
            var (appId, productId) = await FirstApplicationAsync(ctx);
            var doc = await SeedActiveDocumentAsync(ctx, productId);
            submissionId = await SeedSubmissionAsync(ctx, appId, doc);

            // A second, attachable document for the post-publish attempt.
            secondDocId = (await SeedActiveDocumentAsync(ctx, productId)).Id;
        }

        await using (var ctx = New())
        {
            await PublishHandlerFor(ctx).HandleAsync(
                new PublishSubmissionCommand(submissionId), default);
        }

        // Attaching to a published submission is rejected by the aggregate invariant.
        await using var act = New();
        var attach = new AttachProductDocumentHandler(act, new SubmissionRepository(act));

        var call = () => attach.HandleAsync(
            new AttachProductDocumentCommand(submissionId, secondDocId), default);

        await call.Should().ThrowAsync<BusinessRuleViolationException>();
    }
}
