using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Commands.CreateSubmission;
using RegOS.Submission.Application.Commands.PublishSubmission;
using RegOS.Submission.Application.Tests.Fixtures;
using RegOS.Submission.Application.Validation;
using RegOS.Submission.Application.Validation.Models;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;
using RegOS.Submission.Infrastructure.Services;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;

// Two ValidationSeverity enums, in two bounded contexts, that deliberately do
// not share ordinals (ADR-035). Importing the blueprint namespace for section
// types makes the bare name ambiguous — every use here means an *issue's*
// severity, so say so rather than letting the compiler pick.
using ValidationSeverity =
    RegOS.Submission.Application.Validation.Models.ValidationSeverity;

namespace RegOS.Submission.Application.Tests;

// Integration tests — the blueprint judging a real submission against the real
// seeded FDA IND (CTD) template in the dev Postgres.
public sealed class BlueprintValidationTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

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
        var submissionId = await CreateAsync(ctx, appId, "IND coverage");

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

    /// <summary>
    /// Attachment is no longer completeness. This expectation changed
    /// deliberately in EPIC-003: a document that sits nowhere in the dossier
    /// satisfies no placeholder, however right its type. Placement is the unit
    /// of completeness (ADR-036).
    /// </summary>
    [Fact]
    public async Task AttachingWithoutPlacing_ClearsNothing_AndIsDisclosed()
    {
        await using var ctx = New();
        var (appId, globalProductId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, "IND unplaced");

        var before = await MissingCountAsync(ctx, submissionId);

        await AttachAsync(ctx, submissionId, globalProductId, CoverLetter);

        await using var act = New();
        var after = await ValidatorFor(act).ValidateAsync(submissionId, default);

        after.Issues
            .Count(i => i.Code == SubmissionValidationCodes.RequiredDocumentMissing)
            .Should().Be(before, "an unplaced document satisfies nothing");

        // It is not ignored either — attaching something that counts for nothing
        // and hearing nothing about it is how a dossier gets published with a
        // document its author believed was included.
        after.Issues.Should().ContainSingle(
            i => i.Code == SubmissionValidationCodes.DocumentsNotPlaced)
            .Which.Severity.Should().Be(ValidationSeverity.Information);
    }

    [Fact]
    public async Task PlacingARequiredDocumentInItsSection_ClearsItsIssue()
    {
        await using var ctx = New();
        var (appId, globalProductId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, "IND placed");

        var before = await MissingCountAsync(ctx, submissionId);
        var section = await SectionRequiringAsync(ctx, submissionId, CoverLetter);

        await AttachAsync(ctx, submissionId, globalProductId, CoverLetter, section);

        await using var act = New();
        var after = await ValidatorFor(act).ValidateAsync(submissionId, default);

        var afterMissing = after.Issues
            .Where(i => i.Code == SubmissionValidationCodes.RequiredDocumentMissing)
            .ToList();

        afterMissing.Should().HaveCount(before - 1);
        afterMissing.Select(i => i.Message)
            .Should().NotContain(m => m.Contains("Cover Letter"));

        // Placed, so there is nothing to tidy.
        after.Issues.Should().NotContain(
            i => i.Code == SubmissionValidationCodes.DocumentsNotPlaced);
    }

    [Fact]
    public async Task AMisplacedDocument_SatisfiesNothingAndIsNotCalledUnplaced()
    {
        await using var ctx = New();
        var (appId, globalProductId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, "IND misplaced");

        var before = await MissingCountAsync(ctx, submissionId);
        var expected = await SectionRequiringAsync(ctx, submissionId, CoverLetter);
        var elsewhere = await SectionOtherThanAsync(ctx, submissionId, expected);

        await AttachAsync(ctx, submissionId, globalProductId, CoverLetter, elsewhere);

        await using var act = New();
        var after = await ValidatorFor(act).ValidateAsync(submissionId, default);

        after.Issues
            .Count(i => i.Code == SubmissionValidationCodes.RequiredDocumentMissing)
            .Should().Be(before);

        // It is somewhere — legitimate supporting content in the section it was
        // filed into — so the tidy-up disclosure does not apply to it.
        after.Issues.Should().NotContain(
            i => i.Code == SubmissionValidationCodes.DocumentsNotPlaced);
    }

    [Fact]
    public async Task MissingDocumentIssues_NameTheSectionTheyAreExpectedIn()
    {
        await using var ctx = New();
        var (appId, _) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, "IND where");

        var result = await ValidatorFor(ctx).ValidateAsync(submissionId, default);

        // "What" was never enough; now that placement decides the verdict,
        // "where" is half the answer.
        result.Issues
            .Where(i => i.Code == SubmissionValidationCodes.RequiredDocumentMissing)
            .Should().OnlyContain(i => i.Message.Contains(" is missing from "));
    }

    [Fact]
    public async Task UnboundSubmission_IsReportedButNotBlocked()
    {
        await using var ctx = New();
        // A device-type APPLICATION under the same authority: no blueprint
        // targets it. The type moved to the application in S001.
        var (appId, globalProductId) = await TestFdaApplication.Ensure510kAsync(ctx);

        var submissionId = await CreateAsync(ctx, appId, "510(k) unbound");
        await AttachAsync(ctx, submissionId, globalProductId, CoverLetter);

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
        var submissionId = await CreateAsync(ctx, appId, "IND publish gate");

        var handler = new PublishSubmissionHandler(
            ValidatorFor(ctx),
            new SubmissionPublicationBaseline(ctx),
            new SubmissionRepository(ctx));

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
        var (appId, globalProductId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, "IND format");

        await AttachAsync(
            ctx, submissionId, globalProductId, CoverLetter,
            originalFileName: "cover-letter.docx",
            contentType: "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        await using var act = New();
        var result = await ValidatorFor(act).ValidateAsync(submissionId, default);

        // The rule came from the blueprint, and says so. Scoped to this rule:
        // the same blueprint carries SectionNotEmpty rules that also report
        // through BlueprintRuleViolation, and this test is about format.
        var violation = result.Issues.Should().ContainSingle(
            i => i.RuleCode == "FDA-IND-PDF").Subject;

        violation.Code.Should().Be(SubmissionValidationCodes.BlueprintRuleViolation);
        violation.Severity.Should().Be(ValidationSeverity.Error);
        violation.Message.Should().Contain("cover-letter.docx");
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task PdfDocuments_SatisfyTheFormatRule()
    {
        await using var ctx = New();
        var (appId, globalProductId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, "IND format ok");

        await AttachAsync(ctx, submissionId, globalProductId, CoverLetter);

        await using var act = New();
        var result = await ValidatorFor(act).ValidateAsync(submissionId, default);

        result.Issues.Should().NotContain(i => i.RuleCode == "FDA-IND-PDF");
    }

    /// <summary>
    /// The disclosure retires itself. EPIC-002 shipped with an
    /// <c>Information</c> issue naming <c>SectionNotEmpty</c> as unexecutable;
    /// now that every rule type the blueprint carries has an evaluator, it
    /// disappears — without the disclosure mechanism being touched. That is what
    /// makes it a statement about capability rather than a hard-coded caveat.
    /// </summary>
    [Fact]
    public async Task EveryRuleTypeTheSeededBlueprintCarries_IsNowExecuted()
    {
        await using var ctx = New();
        var (appId, _) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, "IND executed");

        var result = await ValidatorFor(ctx).ValidateAsync(submissionId, default);

        result.Issues.Should().NotContain(
            i => i.Code == SubmissionValidationCodes.BlueprintRulesNotEvaluated);
    }

    /// <summary>
    /// And the mechanism still works. Proved by running the engine with no
    /// evaluators at all rather than by relying on a permanently unimplemented
    /// rule type: the invariant worth protecting is that the engine can
    /// distinguish "could not evaluate" from "passed", whatever it ships with.
    /// </summary>
    [Fact]
    public async Task RulesNoEvaluatorClaims_AreStillDisclosed()
    {
        await using var ctx = New();
        var (appId, _) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, "IND disclosure");

        var engineWithNoEvaluators = new BlueprintValidationEvaluator(ctx, []);

        var result = await new SubmissionValidator(
                new SubmissionRepository(ctx), ctx, engineWithNoEvaluators)
            .ValidateAsync(submissionId, default);

        var disclosure = result.Issues.Should().ContainSingle(
            i => i.Code == SubmissionValidationCodes.BlueprintRulesNotEvaluated)
            .Subject;

        // A statement about the engine's capability — not a claim that those
        // rules passed or failed, so it must not block.
        disclosure.Severity.Should().Be(ValidationSeverity.Information);
        disclosure.UnevaluatedRuleTypes
            .Should().Contain(["FileFormat", "SectionNotEmpty"]);
        disclosure.Message.Should().NotContainAny("Error", "Warning");
    }

    /// <summary>
    /// The rule EPIC-002 could only disclose, now doing regulatory work: an
    /// empty Module 1.1 blocks an FDA IND, and the message says which section.
    /// </summary>
    [Fact]
    public async Task AnEmptySectionViolatesTheBlueprintsSectionNotEmptyRule()
    {
        await using var ctx = New();
        var (appId, _) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, "IND empty section");

        var result = await ValidatorFor(ctx).ValidateAsync(submissionId, default);

        var violation = result.Issues.Should().ContainSingle(
            i => i.RuleCode == "FDA-IND-1.1-FORMS-NONEMPTY").Subject;

        violation.Severity.Should().Be(ValidationSeverity.Error);
        violation.Message.Should().Contain("1.1 Forms");

        // The stability rules are graded Warning by the blueprint, so they
        // report without blocking — severity comes from the data, not the code.
        var stability = result.Issues
            .Where(i => i.RuleCode is not null
                && i.RuleCode.EndsWith("STABILITY-NONEMPTY"))
            .ToList();

        stability.Should().HaveCount(2);
        stability.Should().OnlyContain(
            i => i.Severity == ValidationSeverity.Warning);
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
        string title)
    {
        var handler = new CreateSubmissionHandler(ctx, new SubmissionRepository(ctx));

        var result = await handler.HandleAsync(
            new CreateSubmissionCommand(
                applicationId, title + " " + Guid.NewGuid(),
                SubmissionFormat.Ectd,
                TestSubmissionClassification.FdaApplication,
                TestSubmissionClassification.FdaOriginalApplication),
            default);

        _submissionIds.Add(result.Id.Value);

        return result.Id;
    }

    /// <summary>The section the blueprint expects a given document type in.</summary>
    private static async Task<TemplateSectionId> SectionRequiringAsync(
        RegOSDbContext ctx, SubmissionId submissionId, DocumentTypeId documentTypeId)
    {
        var version = await BoundVersionAsync(ctx, submissionId);

        return version.RequiredDocuments
            .First(r => r.DocumentTypeId == documentTypeId)
            .SectionId;
    }

    /// <summary>
    /// A section that expects nothing, so placing there cannot accidentally
    /// satisfy some other placeholder and make the test lie.
    /// </summary>
    private static async Task<TemplateSectionId> SectionOtherThanAsync(
        RegOSDbContext ctx, SubmissionId submissionId, TemplateSectionId exclude)
    {
        var version = await BoundVersionAsync(ctx, submissionId);
        var expectant = version.RequiredDocuments
            .Select(r => r.SectionId)
            .ToHashSet();

        return version.Sections
            .First(s => s.Id != exclude && !expectant.Contains(s.Id))
            .Id;
    }

    private static async Task<RegulatoryTemplateVersion> BoundVersionAsync(
        RegOSDbContext ctx, SubmissionId submissionId)
    {
        var versionId = await ctx.Submissions
            .AsNoTracking()
            .Where(s => s.Id == submissionId)
            .Select(s => s.BoundTemplateVersionId)
            .SingleAsync();

        var template = await ctx.RegulatoryTemplates
            .AsNoTracking()
            .Include(t => t.Versions)
                .ThenInclude(v => v.Sections)
            .Include(t => t.Versions)
                .ThenInclude(v => v.RequiredDocuments)
            .FirstAsync(t => t.Versions.Any(v => v.Id == versionId!.Value));

        return template.Versions.First(v => v.Id == versionId!.Value);
    }

    private async Task AttachAsync(
        RegOSDbContext ctx,
        SubmissionId submissionId,
        GlobalProductId globalProductId,
        DocumentTypeId documentTypeId,
        TemplateSectionId? section = null,
        string originalFileName = "doc.pdf",
        string contentType = "application/pdf")
    {
        var document = ProductDocumentAggregate.Create(
            TestTenant.Id, globalProductId, documentTypeId, "Blueprint Doc " + Guid.NewGuid());

        document.AddInitialVersion(
            originalFileName: originalFileName,
            storedFileName: "v1.pdf",
            contentType: contentType,
            fileSize: 1024,
            storagePath: $"products/{globalProductId.Value}/{document.Id.Value}/v1.pdf",
            checksum: "sha256-x");
        document.Activate();

        ctx.ProductDocuments.Add(document);
        await ctx.SaveChangesAsync();
        _documentIds.Add(document.Id.Value);

        var submission = await ctx.Submissions
            .Include(s => s.Documents)
            .FirstAsync(s => s.Id == submissionId);

        submission.AttachDocument(
            document.Id, document.CurrentVersionId!.Value, section);

        await ctx.SaveChangesAsync();
    }
}
