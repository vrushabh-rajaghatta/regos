using RegOS.SharedKernel.Primitives;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Queries.ValidateSubmission;
using RegOS.Submission.Application.Validation;
using RegOS.Submission.Application.Validation.Models;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;
using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Application.Tests.Fixtures;

namespace RegOS.Submission.Application.Tests;

// Integration tests — exercise the submission validator and the validation query
// against the real dev Postgres (docker postgres-local). Validation reads only; it
// never mutates, so the only cleanup needed is the submissions/documents we seed.
public sealed class ValidateSubmissionTests : IAsyncLifetime
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

    // Remove submissions (with their attachments) before documents, so the
    // RESTRICT FK from SubmissionDocument -> DocumentVersion is satisfied.
    public async Task DisposeAsync()
    {
        await using var ctx = New();

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
            productId, SeededCer, "Validation Doc " + Guid.NewGuid());

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

    // Seeds a submission, optionally attaching one document and/or publishing it.
    private async Task<SubmissionId> SeedSubmissionAsync(
        RegOSDbContext ctx, RegulatoryApplicationId appId,
        ProductDocumentAggregate? document, bool publish)
    {
        var sub = SubmissionAggregate.Create(TenantId.New(), 
            appId, SeededSubmissionType, "Validation Sub " + Guid.NewGuid());

        if (document is not null)
            sub.AttachDocument(document.Id, document.CurrentVersionId!.Value);

        if (publish)
            sub.Publish(DateTimeOffset.UtcNow);

        ctx.Submissions.Add(sub);
        await ctx.SaveChangesAsync();
        _submissionIds.Add(sub.Id.Value);
        return sub.Id;
    }

    private static SubmissionValidator ValidatorFor(RegOSDbContext ctx) =>
        new(new SubmissionRepository(ctx), ctx);

    // --- Validator: issue rules ----------------------------------------------

    [Fact]
    public async Task Validate_DraftWithDocument_IsValid()
    {
        SubmissionId submissionId;
        await using (var ctx = New())
        {
            var (appId, productId) = await FirstApplicationAsync(ctx);
            var doc = await SeedActiveDocumentAsync(ctx, productId);
            submissionId = await SeedSubmissionAsync(ctx, appId, doc, publish: false);
        }

        await using var act = New();
        var result = await ValidatorFor(act).ValidateAsync(submissionId, default);

        result.IsValid.Should().BeTrue();
        result.Issues.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_DraftWithNoDocuments_ReportsHasNoDocuments()
    {
        SubmissionId submissionId;
        await using (var ctx = New())
        {
            var (appId, _) = await FirstApplicationAsync(ctx);
            submissionId = await SeedSubmissionAsync(ctx, appId, document: null, publish: false);
        }

        await using var act = New();
        var result = await ValidatorFor(act).ValidateAsync(submissionId, default);

        result.IsValid.Should().BeFalse();
        result.Issues.Should().ContainSingle()
            .Which.Code.Should().Be(SubmissionValidationCodes.SubmissionHasNoDocuments);
    }

    [Fact]
    public async Task Validate_PublishedSubmission_ReportsAlreadyPublished()
    {
        SubmissionId submissionId;
        await using (var ctx = New())
        {
            var (appId, productId) = await FirstApplicationAsync(ctx);
            var doc = await SeedActiveDocumentAsync(ctx, productId);
            // Otherwise valid (has a document) so only the published rule can fire.
            submissionId = await SeedSubmissionAsync(ctx, appId, doc, publish: true);
        }

        await using var act = New();
        var result = await ValidatorFor(act).ValidateAsync(submissionId, default);

        result.IsValid.Should().BeFalse();
        result.Issues.Should().ContainSingle()
            .Which.Code.Should().Be(SubmissionValidationCodes.SubmissionAlreadyPublished);
    }

    [Fact]
    public async Task Validate_PublishedAndEmpty_ReportsEveryIssue()
    {
        SubmissionId submissionId;
        await using (var ctx = New())
        {
            var (appId, _) = await FirstApplicationAsync(ctx);
            submissionId = await SeedSubmissionAsync(ctx, appId, document: null, publish: true);
        }

        await using var act = New();
        var result = await ValidatorFor(act).ValidateAsync(submissionId, default);

        // The validator reports all problems, not just the first.
        result.IsValid.Should().BeFalse();
        result.Issues.Select(i => i.Code).Should().BeEquivalentTo(new[]
        {
            SubmissionValidationCodes.SubmissionAlreadyPublished,
            SubmissionValidationCodes.SubmissionHasNoDocuments,
        });
    }

    [Fact]
    public async Task Validate_SubmissionNotFound_Throws()
    {
        await using var act = New();

        var call = () => ValidatorFor(act).ValidateAsync(SubmissionId.New(), default);

        await call.Should().ThrowAsync<NotFoundException>();
    }

    // --- Query handler: response contract ------------------------------------

    [Fact]
    public async Task Handler_MapsResultToResponseContract()
    {
        SubmissionId submissionId;
        await using (var ctx = New())
        {
            var (appId, _) = await FirstApplicationAsync(ctx);
            submissionId = await SeedSubmissionAsync(ctx, appId, document: null, publish: false);
        }

        await using var act = New();
        var handler = new ValidateSubmissionHandler(ValidatorFor(act));

        var response = await handler.HandleAsync(submissionId, default);

        response.IsValid.Should().BeFalse();
        response.Issues.Should().ContainSingle();
        var issue = response.Issues.Single();
        issue.Code.Should().Be(SubmissionValidationCodes.SubmissionHasNoDocuments);
        issue.Severity.Should().Be(ValidationSeverity.Error);
    }
}
