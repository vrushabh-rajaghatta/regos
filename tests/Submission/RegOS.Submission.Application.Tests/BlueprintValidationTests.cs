using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Commands.CreateSubmission;
using RegOS.Submission.Application.Commands.PublishSubmission;
using RegOS.Submission.Application.Tests.Fixtures;
using RegOS.Submission.Application.Validation;
using RegOS.Submission.Application.Validation.Models;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;

namespace RegOS.Submission.Application.Tests;

// Integration tests — the blueprint judging a real submission against the real
// seeded FDA IND (CTD) template in the dev Postgres.
public sealed class BlueprintValidationTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private static readonly SubmissionTypeId FdaInd =
        new(Guid.Parse("40000000-0000-0000-0000-000000000008"));
    private static readonly SubmissionTypeId Fda510k =
        new(Guid.Parse("40000000-0000-0000-0000-000000000001"));
    private static readonly DocumentTypeId CoverLetter =
        new(Guid.Parse("50000000-0000-0000-0000-000000000009"));

    private readonly List<Guid> _submissionIds = [];
    private readonly List<Guid> _documentIds = [];

    private static DbContextOptions<RegOSDbContext> Options() =>
        new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

    private static RegOSDbContext New() => new(Options(), TestTenant.Context);

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var ctx = New();

        if (_submissionIds.Count > 0)
        {
            var ids = _submissionIds.ToArray();

            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"SubmissionSnapshotDocuments\" WHERE \"SubmissionSnapshotId\" IN "
                    + "(SELECT \"Id\" FROM \"SubmissionSnapshots\" WHERE \"SubmissionId\" = ANY({0}))",
                new object[] { ids });
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"SubmissionSnapshots\" WHERE \"SubmissionId\" = ANY({0})",
                new object[] { ids });
        }

        foreach (var id in _submissionIds)
        {
            var submission = await ctx.Submissions
                .Include(s => s.Documents)
                .FirstOrDefaultAsync(s => s.Id == new SubmissionId(id));

            if (submission is not null)
                ctx.Submissions.Remove(submission);
        }

        await ctx.SaveChangesAsync();

        foreach (var id in _documentIds)
        {
            var document = await ctx.ProductDocuments
                .Include(d => d.Versions)
                .FirstOrDefaultAsync(d => d.Id == new ProductDocumentId(id));

            if (document is not null)
                ctx.ProductDocuments.Remove(document);
        }

        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task BoundSubmissionWithNoDocuments_ReportsEveryRequiredDocument()
    {
        await using var ctx = New();
        var (appId, _) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, FdaInd, "IND coverage");

        var result = await ValidatorFor(ctx).ValidateAsync(submissionId, default);

        var missing = result.Issues
            .Where(i => i.Code == SubmissionValidationCodes.RequiredDocumentMissing)
            .ToList();

        // The blueprint — not code here — decides how many documents are owed.
        missing.Should().NotBeEmpty();
        missing.Should().OnlyContain(i => i.Severity == ValidationSeverity.Error);
        result.IsValid.Should().BeFalse();

        // Issues name the document, so a person can act on them.
        missing.Select(i => i.Message)
            .Should().Contain(m => m.Contains("Cover Letter"));
    }

    [Fact]
    public async Task AttachingARequiredDocument_ClearsItsIssue()
    {
        await using var ctx = New();
        var (appId, productId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, FdaInd, "IND partial");

        var before = await MissingCountAsync(ctx, submissionId);

        await AttachAsync(ctx, submissionId, productId, CoverLetter);

        await using var act = New();
        var after = await ValidatorFor(act).ValidateAsync(submissionId, default);

        var afterMissing = after.Issues
            .Where(i => i.Code == SubmissionValidationCodes.RequiredDocumentMissing)
            .ToList();

        afterMissing.Should().HaveCount(before - 1);
        afterMissing.Select(i => i.Message)
            .Should().NotContain(m => m.Contains("Cover Letter"));
    }

    [Fact]
    public async Task UnboundSubmission_IsReportedButNotBlocked()
    {
        await using var ctx = New();
        var (appId, productId) = await TestFdaApplication.EnsureAsync(ctx);

        // A device type under the same authority: no blueprint targets it.
        var submissionId = await CreateAsync(ctx, appId, Fda510k, "510(k) unbound");
        await AttachAsync(ctx, submissionId, productId, CoverLetter);

        await using var act = New();
        var result = await ValidatorFor(act).ValidateAsync(submissionId, default);

        var notBound = result.Issues.Should().ContainSingle(
            i => i.Code == SubmissionValidationCodes.SubmissionNotBoundToBlueprint)
            .Subject;

        // Visible, so "not checked" cannot be mistaken for "checked and clean" —
        // but it does not stop a submission that is otherwise ready.
        notBound.Severity.Should().Be(ValidationSeverity.Information);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task PublishingIsBlockedWhileRequiredDocumentsAreMissing()
    {
        await using var ctx = New();
        var (appId, _) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, FdaInd, "IND publish gate");

        var handler = new PublishSubmissionHandler(
            ValidatorFor(ctx),
            new SubmissionRepository(ctx),
            new SubmissionSnapshotRepository(ctx));

        var result = await handler.HandleAsync(
            new PublishSubmissionCommand(submissionId), default);

        result.Published.Should().BeFalse();
        result.Validation!.Issues
            .Should().Contain(i =>
                i.Code == SubmissionValidationCodes.RequiredDocumentMissing);

        // And it really did not publish.
        await using var check = New();
        var submission = await check.Submissions
            .AsNoTracking()
            .FirstAsync(s => s.Id == submissionId);
        submission.Status.Should().Be(SubmissionStatus.Draft);
    }

    [Fact]
    public async Task NonPdfDocument_ViolatesTheBlueprintsFormatRule()
    {
        await using var ctx = New();
        var (appId, productId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, FdaInd, "IND format");

        await AttachAsync(
            ctx, submissionId, productId, CoverLetter,
            originalFileName: "cover-letter.docx",
            contentType: "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        await using var act = New();
        var result = await ValidatorFor(act).ValidateAsync(submissionId, default);

        // The rule came from the blueprint, and says so.
        var violation = result.Issues.Should().ContainSingle(
            i => i.Code == SubmissionValidationCodes.BlueprintRuleViolation).Subject;

        violation.RuleCode.Should().Be("FDA-IND-PDF");
        violation.Severity.Should().Be(ValidationSeverity.Error);
        violation.Message.Should().Contain("cover-letter.docx");
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task PdfDocuments_SatisfyTheFormatRule()
    {
        await using var ctx = New();
        var (appId, productId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, FdaInd, "IND format ok");

        await AttachAsync(ctx, submissionId, productId, CoverLetter);

        await using var act = New();
        var result = await ValidatorFor(act).ValidateAsync(submissionId, default);

        result.Issues.Should().NotContain(
            i => i.Code == SubmissionValidationCodes.BlueprintRuleViolation);
    }

    [Fact]
    public async Task RuleTypesTheEngineCannotRunYet_AreDisclosed()
    {
        await using var ctx = New();
        var (appId, _) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, FdaInd, "IND disclosure");

        var result = await ValidatorFor(ctx).ValidateAsync(submissionId, default);

        var disclosure = result.Issues.Should().ContainSingle(
            i => i.Code == SubmissionValidationCodes.BlueprintRulesNotEvaluated)
            .Subject;

        // A statement about this engine's capability — not a claim that those
        // rules passed or failed, so it must not block.
        disclosure.Severity.Should().Be(ValidationSeverity.Information);
        disclosure.UnevaluatedRuleTypes.Should().Contain("SectionNotEmpty");
        disclosure.Message.Should().NotContainAny("Error", "Warning");
    }

    // --- helpers -------------------------------------------------------------

    private static SubmissionValidator ValidatorFor(RegOSDbContext ctx) =>
        new(new SubmissionRepository(ctx), ctx);

    private async Task<int> MissingCountAsync(
        RegOSDbContext ctx, SubmissionId submissionId)
    {
        var result = await ValidatorFor(ctx).ValidateAsync(submissionId, default);

        return result.Issues
            .Count(i => i.Code == SubmissionValidationCodes.RequiredDocumentMissing);
    }

    private async Task<SubmissionId> CreateAsync(
        RegOSDbContext ctx,
        RegulatoryApplicationId applicationId,
        SubmissionTypeId submissionTypeId,
        string title)
    {
        var handler = new CreateSubmissionHandler(ctx, new SubmissionRepository(ctx));

        var result = await handler.HandleAsync(
            new CreateSubmissionCommand(
                applicationId, submissionTypeId, title + " " + Guid.NewGuid()),
            default);

        _submissionIds.Add(result.Id.Value);

        return result.Id;
    }

    private async Task AttachAsync(
        RegOSDbContext ctx,
        SubmissionId submissionId,
        ProductId productId,
        DocumentTypeId documentTypeId,
        string originalFileName = "doc.pdf",
        string contentType = "application/pdf")
    {
        var document = ProductDocumentAggregate.Create(
            TestTenant.Id, productId, documentTypeId, "Blueprint Doc " + Guid.NewGuid());

        document.AddInitialVersion(
            originalFileName: originalFileName,
            storedFileName: "v1.pdf",
            contentType: contentType,
            fileSize: 1024,
            storagePath: $"products/{productId.Value}/{document.Id.Value}/v1.pdf",
            checksum: "sha256-x");
        document.Activate();

        ctx.ProductDocuments.Add(document);
        await ctx.SaveChangesAsync();
        _documentIds.Add(document.Id.Value);

        var submission = await ctx.Submissions
            .Include(s => s.Documents)
            .FirstAsync(s => s.Id == submissionId);

        submission.AttachDocument(document.Id, document.CurrentVersionId!.Value);

        await ctx.SaveChangesAsync();
    }
}
