using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Commands.AttachProductDocument;
using RegOS.Submission.Application.Commands.RemoveProductDocument;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;
using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Submission.Application.Tests;

// Integration tests — exercise the attach/remove handlers end-to-end against
// the real dev Postgres (docker postgres-local). This is the first point at
// which the Submission and Product Document capabilities run together.
public sealed class AttachRemoveProductDocumentTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    // Seeded reference data.
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

    // Remove attachments (via their submissions) before documents, so the
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
        var app = await ctx.RegulatoryApplications.AsNoTracking().FirstAsync();
        return (app.Id, app.ProductId);
    }

    private async Task<ProductDocumentAggregate> SeedDocumentAsync(
        RegOSDbContext ctx, ProductId productId, bool activate)
    {
        var doc = ProductDocumentAggregate.Create(
            productId, SeededCer, "19.3 Doc " + Guid.NewGuid());

        doc.AddInitialVersion(
            originalFileName: "cer.pdf",
            storedFileName: "v1.pdf",
            contentType: "application/pdf",
            fileSize: 1024,
            storagePath: $"products/{productId.Value}/{doc.Id.Value}/v1.pdf",
            checksum: "sha256-x");

        if (activate)
            doc.Activate();

        ctx.ProductDocuments.Add(doc);
        await ctx.SaveChangesAsync();
        _documentIds.Add(doc.Id.Value);
        return doc;
    }

    private async Task<SubmissionAggregate> SeedSubmissionAsync(
        RegOSDbContext ctx, RegulatoryApplicationId appId)
    {
        var sub = SubmissionAggregate.Create(
            appId, SeededSubmissionType, "19.3 Sub " + Guid.NewGuid());

        ctx.Submissions.Add(sub);
        await ctx.SaveChangesAsync();
        _submissionIds.Add(sub.Id.Value);
        return sub;
    }

    // --- Attach: full flow ---------------------------------------------------

    [Fact]
    public async Task Attach_ActiveDocument_PersistsAttachmentWithCurrentVersion()
    {
        SubmissionId submissionId;
        ProductDocumentId documentId;
        DocumentVersionId versionId;

        await using (var ctx = New())
        {
            var (appId, productId) = await FirstApplicationAsync(ctx);
            var doc = await SeedDocumentAsync(ctx, productId, activate: true);
            var sub = await SeedSubmissionAsync(ctx, appId);

            submissionId = sub.Id;
            documentId = doc.Id;
            versionId = doc.CurrentVersionId!.Value;
        }

        // Act — a fresh context, as a real request would have.
        await using (var ctx = New())
        {
            var handler = new AttachProductDocumentHandler(
                ctx, new SubmissionRepository(ctx));

            var result = await handler.HandleAsync(
                new AttachProductDocumentCommand(submissionId, documentId),
                default);

            result.Id.Value.Should().NotBe(Guid.Empty);
        }

        // Assert — reload from a fresh context.
        await using (var ctx = New())
        {
            var reloaded = await new SubmissionRepository(ctx)
                .GetByIdAsync(submissionId, default);

            reloaded.Should().NotBeNull();
            reloaded!.Documents.Should().ContainSingle();

            var attachment = reloaded.Documents.Single();
            attachment.ProductDocumentId.Should().Be(documentId);
            attachment.DocumentVersionId.Should().Be(versionId);
            attachment.DisplayOrder.Should().Be(1);
        }
    }

    // --- Attach: validation --------------------------------------------------

    [Fact]
    public async Task Attach_UnknownProductDocument_ThrowsInvalidRequest()
    {
        SubmissionId submissionId;
        await using (var ctx = New())
        {
            var (appId, _) = await FirstApplicationAsync(ctx);
            submissionId = (await SeedSubmissionAsync(ctx, appId)).Id;
        }

        await using var act = New();
        var handler = new AttachProductDocumentHandler(
            act, new SubmissionRepository(act));

        var call = () => handler.HandleAsync(
            new AttachProductDocumentCommand(
                submissionId, ProductDocumentId.New()),
            default);

        await call.Should().ThrowAsync<DomainException>()
            .WithMessage(SubmissionRuleErrors.ProductDocumentDoesNotExist);
    }

    [Fact]
    public async Task Attach_InactiveProductDocument_ThrowsBusinessRule()
    {
        SubmissionId submissionId;
        ProductDocumentId documentId;

        await using (var ctx = New())
        {
            var (appId, productId) = await FirstApplicationAsync(ctx);
            // Draft — never activated.
            documentId = (await SeedDocumentAsync(ctx, productId, activate: false)).Id;
            submissionId = (await SeedSubmissionAsync(ctx, appId)).Id;
        }

        await using var act = New();
        var handler = new AttachProductDocumentHandler(
            act, new SubmissionRepository(act));

        var call = () => handler.HandleAsync(
            new AttachProductDocumentCommand(submissionId, documentId),
            default);

        await call.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(SubmissionRuleErrors.ProductDocumentNotActive);
    }

    [Fact]
    public async Task Attach_DocumentFromAnotherProduct_ThrowsInvalidRequest()
    {
        SubmissionId submissionId;
        ProductDocumentId documentId;

        await using (var ctx = New())
        {
            var (appId, productId) = await FirstApplicationAsync(ctx);

            // A different product than the submission's application owns.
            var otherProductId = await ctx.Products
                .Where(p => p.Id != productId)
                .Select(p => p.Id)
                .FirstAsync();

            documentId = (await SeedDocumentAsync(ctx, otherProductId, activate: true)).Id;
            submissionId = (await SeedSubmissionAsync(ctx, appId)).Id;
        }

        await using var act = New();
        var handler = new AttachProductDocumentHandler(
            act, new SubmissionRepository(act));

        var call = () => handler.HandleAsync(
            new AttachProductDocumentCommand(submissionId, documentId),
            default);

        await call.Should().ThrowAsync<DomainException>()
            .WithMessage(SubmissionRuleErrors.ProductDocumentNotInSameProduct);
    }

    [Fact]
    public async Task Attach_SubmissionNotFound_ThrowsSubmissionNotFound()
    {
        await using var act = New();
        var handler = new AttachProductDocumentHandler(
            act, new SubmissionRepository(act));

        var call = () => handler.HandleAsync(
            new AttachProductDocumentCommand(
                SubmissionId.New(), ProductDocumentId.New()),
            default);

        await call.Should().ThrowAsync<NotFoundException>()
            .WithMessage(SubmissionRuleErrors.SubmissionDoesNotExist);
    }

    [Fact]
    public async Task Attach_Duplicate_ThrowsInvalidOperation()
    {
        SubmissionId submissionId;
        ProductDocumentId documentId;

        await using (var ctx = New())
        {
            var (appId, productId) = await FirstApplicationAsync(ctx);
            documentId = (await SeedDocumentAsync(ctx, productId, activate: true)).Id;
            submissionId = (await SeedSubmissionAsync(ctx, appId)).Id;
        }

        // First attach succeeds.
        await using (var ctx = New())
        {
            await new AttachProductDocumentHandler(ctx, new SubmissionRepository(ctx))
                .HandleAsync(
                    new AttachProductDocumentCommand(submissionId, documentId),
                    default);
        }

        // Second attach of the same document is rejected by the aggregate.
        await using (var ctx = New())
        {
            var handler = new AttachProductDocumentHandler(
                ctx, new SubmissionRepository(ctx));

            var call = () => handler.HandleAsync(
                new AttachProductDocumentCommand(submissionId, documentId),
                default);

            await call.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage(SubmissionErrors.ProductDocumentAlreadyAttached);
        }
    }

    // --- Remove --------------------------------------------------------------

    [Fact]
    public async Task Remove_ExistingAttachment_RemovesIt()
    {
        SubmissionId submissionId;
        ProductDocumentId documentId;

        await using (var ctx = New())
        {
            var (appId, productId) = await FirstApplicationAsync(ctx);
            documentId = (await SeedDocumentAsync(ctx, productId, activate: true)).Id;
            submissionId = (await SeedSubmissionAsync(ctx, appId)).Id;
        }

        SubmissionDocumentId attachmentId;
        await using (var ctx = New())
        {
            attachmentId = (await new AttachProductDocumentHandler(
                    ctx, new SubmissionRepository(ctx))
                .HandleAsync(
                    new AttachProductDocumentCommand(submissionId, documentId),
                    default)).Id;
        }

        await using (var ctx = New())
        {
            await new RemoveProductDocumentHandler(new SubmissionRepository(ctx))
                .HandleAsync(
                    new RemoveProductDocumentCommand(submissionId, attachmentId),
                    default);
        }

        await using (var ctx = New())
        {
            var reloaded = await new SubmissionRepository(ctx)
                .GetByIdAsync(submissionId, default);
            reloaded!.Documents.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Remove_UnknownAttachment_ThrowsInvalidOperation()
    {
        SubmissionId submissionId;
        await using (var ctx = New())
        {
            var (appId, _) = await FirstApplicationAsync(ctx);
            submissionId = (await SeedSubmissionAsync(ctx, appId)).Id;
        }

        await using var act = New();
        var handler = new RemoveProductDocumentHandler(new SubmissionRepository(act));

        var call = () => handler.HandleAsync(
            new RemoveProductDocumentCommand(
                submissionId, SubmissionDocumentId.New()),
            default);

        await call.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.DocumentNotAttached);
    }

    [Fact]
    public async Task Remove_SubmissionNotFound_ThrowsSubmissionNotFound()
    {
        await using var act = New();
        var handler = new RemoveProductDocumentHandler(new SubmissionRepository(act));

        var call = () => handler.HandleAsync(
            new RemoveProductDocumentCommand(
                SubmissionId.New(), SubmissionDocumentId.New()),
            default);

        await call.Should().ThrowAsync<NotFoundException>()
            .WithMessage(SubmissionRuleErrors.SubmissionDoesNotExist);
    }
}
