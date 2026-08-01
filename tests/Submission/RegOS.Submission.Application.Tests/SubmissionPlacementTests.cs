using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Application.Commands.AttachProductDocument;
using RegOS.Submission.Application.Commands.CreateSubmission;
using RegOS.Submission.Application.Commands.PlaceSubmissionDocument;
using RegOS.Submission.Application.Queries.GetSubmissionContentPlan;
using RegOS.Submission.Application.Tests.Fixtures;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// Placement and the content plan, against the real seeded FDA IND (CTD)
/// blueprint in the dev Postgres.
/// </summary>
/// <remarks>
/// The blueprint supplies the section ids these tests use — nothing is
/// hard-coded beyond the submission types and the Cover Letter document type,
/// so the tests keep meaning what they say as the template grows.
/// </remarks>
public sealed class SubmissionPlacementTests : IAsyncLifetime
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

    private static RegOSDbContext New() =>
        new(
            new DbContextOptionsBuilder<RegOSDbContext>()
                .UseNpgsql(ConnectionString)
                .Options,
            TestTenant.Context);

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

    // --- the rule the aggregate cannot enforce -------------------------------

    [Fact]
    public async Task AttachingIntoASectionOfTheBoundBlueprint_PlacesTheDocument()
    {
        await using var ctx = New();
        var (appId, globalProductId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, FdaInd, "IND place");
        var section = await SectionRequiringAsync(ctx, submissionId, CoverLetter);

        var attachmentId = await AttachAsync(
            ctx, submissionId, globalProductId, CoverLetter, section);

        await using var check = New();
        var placement = await PlacementOfAsync(check, submissionId, attachmentId);

        placement.Should().Be(section);
    }

    [Fact]
    public async Task AttachingIntoASectionOfAnotherBlueprint_IsRejected()
    {
        await using var ctx = New();
        var (appId, globalProductId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, FdaInd, "IND foreign");
        var foreign = await SectionOfAnotherVersionAsync(ctx, submissionId);

        var attach = async () => await AttachAsync(
            ctx, submissionId, globalProductId, CoverLetter, foreign);

        await attach.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(SubmissionRuleErrors.TemplateSectionNotInBoundBlueprint);

        // And the attachment was not made either: the placement is checked
        // before the document joins the dossier.
        await using var check = New();
        var documents = await check.Submissions
            .AsNoTracking()
            .Where(s => s.Id == submissionId)
            .SelectMany(s => s.Documents)
            .CountAsync();

        documents.Should().Be(0);
    }

    [Fact]
    public async Task PlacingOnASubmissionWithNoBlueprint_IsRejected()
    {
        await using var ctx = New();
        var (appId, globalProductId) = await TestFdaApplication.EnsureAsync(ctx);

        // A device type under the same authority — no blueprint targets it.
        var submissionId = await CreateAsync(ctx, appId, Fda510k, "510(k) place");
        var attachmentId = await AttachAsync(
            ctx, submissionId, globalProductId, CoverLetter);

        // Any section at all: there is no structure to place into.
        var anySection = await AnySectionAsync(ctx);

        await using var act = New();
        var place = async () => await PlaceAsync(
            act, submissionId, attachmentId, anySection);

        await place.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(SubmissionRuleErrors.SubmissionHasNoBlueprintToPlaceInto);
    }

    [Fact]
    public async Task PlacingMovesAnAlreadyPlacedDocument()
    {
        await using var ctx = New();
        var (appId, globalProductId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, FdaInd, "IND move");
        var origin = await SectionRequiringAsync(ctx, submissionId, CoverLetter);
        var attachmentId = await AttachAsync(
            ctx, submissionId, globalProductId, CoverLetter, origin);

        var destination = await SectionOtherThanAsync(ctx, submissionId, origin);

        await using var act = New();
        await PlaceAsync(act, submissionId, attachmentId, destination);

        await using var check = New();
        var placement = await PlacementOfAsync(check, submissionId, attachmentId);

        placement.Should().Be(destination);
    }

    [Fact]
    public async Task ClearingAPlacementLeavesTheDocumentAttached()
    {
        await using var ctx = New();
        var (appId, globalProductId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, FdaInd, "IND clear");
        var section = await SectionRequiringAsync(ctx, submissionId, CoverLetter);
        var attachmentId = await AttachAsync(
            ctx, submissionId, globalProductId, CoverLetter, section);

        await using var act = New();
        await PlaceAsync(act, submissionId, attachmentId, section: null);

        await using var check = New();
        var plan = await PlanAsync(check, submissionId);

        plan!.UnplacedDocuments.Should().ContainSingle()
            .Which.SubmissionDocumentId.Should().Be(attachmentId.Value);
    }

    // --- the content plan ----------------------------------------------------

    [Fact]
    public async Task APlaceholderIsSatisfiedByTheRightTypeInItsOwnSection()
    {
        await using var ctx = New();
        var (appId, globalProductId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, FdaInd, "IND satisfy");
        var section = await SectionRequiringAsync(ctx, submissionId, CoverLetter);

        await AttachAsync(ctx, submissionId, globalProductId, CoverLetter, section);

        await using var check = New();
        var plan = await PlanAsync(check, submissionId);

        var placeholder = Placeholders(plan!)
            .Single(p => p.DocumentTypeId == CoverLetter.Value);

        placeholder.IsSatisfied.Should().BeTrue();
        placeholder.Documents.Should().ContainSingle();
        placeholder.DocumentTypeName.Should().Be("Cover Letter");
        plan!.UnplacedDocuments.Should().BeEmpty();
    }

    /// <summary>
    /// The whole point of placement: the right document in the wrong section
    /// satisfies nothing. Under EPIC-002's type-only coverage this would have
    /// counted.
    /// </summary>
    [Fact]
    public async Task TheRightTypeInTheWrongSectionSatisfiesNothing()
    {
        await using var ctx = New();
        var (appId, globalProductId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, FdaInd, "IND misplaced");
        var expected = await SectionRequiringAsync(ctx, submissionId, CoverLetter);
        var elsewhere = await SectionOtherThanAsync(ctx, submissionId, expected);

        await AttachAsync(ctx, submissionId, globalProductId, CoverLetter, elsewhere);

        await using var check = New();
        var plan = await PlanAsync(check, submissionId);

        Placeholders(plan!)
            .Single(p => p.DocumentTypeId == CoverLetter.Value)
            .IsSatisfied.Should().BeFalse();

        // It is not lost, though — it is legitimate supporting content in the
        // section it was actually placed into.
        Sections(plan!)
            .Single(s => s.SectionId == elsewhere.Value)
            .AdditionalDocuments.Should().ContainSingle();
    }

    [Fact]
    public async Task AnAttachedButUnplacedDocumentIsListedSeparately()
    {
        await using var ctx = New();
        var (appId, globalProductId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, FdaInd, "IND unplaced");

        await AttachAsync(ctx, submissionId, globalProductId, CoverLetter);

        await using var check = New();
        var plan = await PlanAsync(check, submissionId);

        plan!.UnplacedDocuments.Should().ContainSingle();

        Placeholders(plan)
            .Single(p => p.DocumentTypeId == CoverLetter.Value)
            .IsSatisfied.Should().BeFalse();
    }

    [Fact]
    public async Task TheStructureComesFromTheBlueprint()
    {
        await using var ctx = New();
        var (appId, _) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, FdaInd, "IND structure");

        await using var check = New();
        var plan = await PlanAsync(check, submissionId);

        plan!.BoundTemplateVersionId.Should().NotBeNull();
        plan.TemplateName.Should().NotBeNullOrWhiteSpace();

        // A tree, not a flat list — modules own the sections beneath them.
        plan.Sections.Should().NotBeEmpty();
        plan.Sections.Should().Contain(s => s.Children.Count > 0);

        // Every placeholder the blueprint declares is present and empty.
        var placeholders = Placeholders(plan).ToList();
        placeholders.Should().NotBeEmpty();
        placeholders.Should().OnlyContain(p => !p.IsSatisfied);
    }

    [Fact]
    public async Task ASubmissionWithNoBlueprintGetsAnEmptyStructure()
    {
        await using var ctx = New();
        var (appId, globalProductId) = await TestFdaApplication.EnsureAsync(ctx);
        var submissionId = await CreateAsync(ctx, appId, Fda510k, "510(k) plan");

        await AttachAsync(ctx, submissionId, globalProductId, CoverLetter);

        await using var check = New();
        var plan = await PlanAsync(check, submissionId);

        // An envelope, not a 404: "no blueprint governs this" is a state the UI
        // renders, not a failure to report.
        plan.Should().NotBeNull();
        plan!.BoundTemplateVersionId.Should().BeNull();
        plan.Sections.Should().BeEmpty();
        plan.UnplacedDocuments.Should().ContainSingle();
    }

    [Fact]
    public async Task AMissingSubmissionHasNoContentPlan()
    {
        await using var ctx = New();

        var plan = await PlanAsync(ctx, SubmissionId.New());

        plan.Should().BeNull();
    }

    // --- helpers -------------------------------------------------------------

    private static IEnumerable<ContentPlanSection> Sections(
        SubmissionContentPlan plan) => Flatten(plan.Sections);

    private static IEnumerable<ContentPlanSection> Flatten(
        IEnumerable<ContentPlanSection> sections) =>
        sections.SelectMany(s => new[] { s }.Concat(Flatten(s.Children)));

    private static IEnumerable<ContentPlanPlaceholder> Placeholders(
        SubmissionContentPlan plan) =>
        Sections(plan).SelectMany(s => s.Placeholders);

    private static Task<SubmissionContentPlan?> PlanAsync(
        RegOSDbContext ctx, SubmissionId submissionId) =>
        new GetSubmissionContentPlanHandler(ctx).HandleAsync(submissionId, default);

    private static async Task<TemplateSectionId?> PlacementOfAsync(
        RegOSDbContext ctx, SubmissionId submissionId, SubmissionDocumentId id) =>
        await ctx.Submissions
            .AsNoTracking()
            .Where(s => s.Id == submissionId)
            .SelectMany(s => s.Documents)
            .Where(d => d.Id == id)
            .Select(d => d.TemplateSectionId)
            .SingleAsync();

    /// <summary>The section the blueprint expects a given document type in.</summary>
    private static async Task<TemplateSectionId> SectionRequiringAsync(
        RegOSDbContext ctx, SubmissionId submissionId, DocumentTypeId documentTypeId)
    {
        var version = await BoundVersionAsync(ctx, submissionId);

        return version.RequiredDocuments
            .First(r => r.DocumentTypeId == documentTypeId)
            .SectionId;
    }

    private static async Task<TemplateSectionId> SectionOtherThanAsync(
        RegOSDbContext ctx, SubmissionId submissionId, TemplateSectionId exclude)
    {
        var version = await BoundVersionAsync(ctx, submissionId);
        var requiredTypes = version.RequiredDocuments
            .Where(r => r.SectionId != exclude)
            .Select(r => r.SectionId)
            .ToHashSet();

        // A section that expects nothing, so placing here cannot accidentally
        // satisfy a different placeholder and make the test lie.
        return version.Sections
            .First(s => s.Id != exclude && !requiredTypes.Contains(s.Id))
            .Id;
    }

    private static async Task<TemplateSectionId> SectionOfAnotherVersionAsync(
        RegOSDbContext ctx, SubmissionId submissionId)
    {
        var boundVersionId = await ctx.Submissions
            .AsNoTracking()
            .Where(s => s.Id == submissionId)
            .Select(s => s.BoundTemplateVersionId)
            .SingleAsync();

        var section = await ctx.RegulatoryTemplates
            .AsNoTracking()
            .Include(t => t.Versions)
                .ThenInclude(v => v.Sections)
            .SelectMany(t => t.Versions)
            .Where(v => v.Id != boundVersionId!.Value)
            .SelectMany(v => v.Sections)
            .FirstOrDefaultAsync();

        section.Should().NotBeNull(
            "another seeded blueprint is needed to prove the boundary is enforced");

        return section!.Id;
    }

    private static async Task<TemplateSectionId> AnySectionAsync(RegOSDbContext ctx) =>
        (await ctx.RegulatoryTemplates
            .AsNoTracking()
            .Include(t => t.Versions)
                .ThenInclude(v => v.Sections)
            .SelectMany(t => t.Versions)
            .SelectMany(v => v.Sections)
            .FirstAsync()).Id;

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

    /// <summary>
    /// Goes through the real handler, so the cross-context placement rule runs.
    /// </summary>
    private async Task<SubmissionDocumentId> AttachAsync(
        RegOSDbContext ctx,
        SubmissionId submissionId,
        GlobalProductId globalProductId,
        DocumentTypeId documentTypeId,
        TemplateSectionId? section = null)
    {
        var document = ProductDocumentAggregate.Create(
            TestTenant.Id, globalProductId, documentTypeId, "Placement Doc " + Guid.NewGuid());

        document.AddInitialVersion(
            originalFileName: "doc.pdf",
            storedFileName: "v1.pdf",
            contentType: "application/pdf",
            fileSize: 1024,
            storagePath: $"products/{globalProductId.Value}/{document.Id.Value}/v1.pdf",
            checksum: "sha256-x");
        document.Activate();

        ctx.ProductDocuments.Add(document);
        await ctx.SaveChangesAsync();
        _documentIds.Add(document.Id.Value);

        await using var act = New();

        var result = await new AttachProductDocumentHandler(
                act, new SubmissionRepository(act))
            .HandleAsync(
                new AttachProductDocumentCommand(submissionId, document.Id, section),
                default);

        return result.Id;
    }

    private static Task PlaceAsync(
        RegOSDbContext ctx,
        SubmissionId submissionId,
        SubmissionDocumentId documentId,
        TemplateSectionId? section) =>
        new PlaceSubmissionDocumentHandler(ctx, new SubmissionRepository(ctx))
            .HandleAsync(
                new PlaceSubmissionDocumentCommand(submissionId, documentId, section),
                default);
}
