using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Commands.CreateSubmission;
using RegOS.Submission.Application.Queries.GetSubmission;
using RegOS.Submission.Application.Tests.Fixtures;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;

using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;
using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Application.Tests;

// Integration tests — exercise blueprint resolution against the real dev
// Postgres and its seeded reference data (docker postgres-local).
//
// The pair of cases is the point: two application types under the SAME
// authority, one of which a published blueprint targets (FDA IND) and one of
// which none does (FDA 510(k)). That isolates "was a template found?" from
// "does the authority match?".
//
// Since EPIC-007a S001 the pair is two APPLICATIONS rather than two submissions
// under one: the type classifies the application, so a submission inherits the
// blueprint its application's type resolves to and cannot choose another.
[Collection(SubmissionDatabase.Collection)]
public sealed class CreateSubmissionBindingTests : IAsyncLifetime
{
    private readonly SubmissionDatabase _database;

    public CreateSubmissionBindingTests(SubmissionDatabase database)
    {
        _database = database;
    }


    private static readonly AuthorityId Fda =
        new(Guid.Parse("20000000-0000-0000-0000-000000000001"));

    private static readonly RegulatoryTemplateId FdaIndCtd =
        new(Guid.Parse("60000000-0000-0000-0000-000000000001"));

    private readonly List<Guid> _submissionIds = [];

    private DbContextOptions<RegOSDbContext> Options() =>
        _database.Options;

    private RegOSDbContext New() => new(Options(), TestTenant.Context);

    public Task InitializeAsync() => Task.CompletedTask;

    // Only the submissions these tests create are removed. The parent
    // product/application is a shared fixture, kept for the same reason
    // TestApplications keeps its own.
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
    }

    [Fact]
    public async Task Create_BindsTheSubmissionToThePublishedBlueprint()
    {
        await using var ctx = New();
        var applicationId = await IndApplicationAsync(ctx);

        var submission = await CreateAsync(ctx, applicationId, "IND binding");

        submission.BoundTemplateVersionId.Should().NotBeNull();

        // It is the published version of the FDA IND (CTD) blueprint.
        var template = await ctx.RegulatoryTemplates
            .AsNoTracking()
            .Include(t => t.Versions)
            .FirstAsync(t => t.Id == FdaIndCtd);

        template.Versions
            .Select(v => v.Id)
            .Should().Contain(submission.BoundTemplateVersionId!.Value);
    }

    [Fact]
    public async Task Create_WithNoBlueprintForTheType_LeavesTheSubmissionUnbound()
    {
        await using var ctx = New();
        var applicationId = await DeviceApplicationAsync(ctx);

        var submission = await CreateAsync(ctx, applicationId, "510(k) binding");

        // Reference data that has no published blueprint must not block
        // creating a submission.
        submission.BoundTemplateVersionId.Should().BeNull();
    }

    [Fact]
    public async Task GetSubmission_ExposesTheBoundBlueprintForDisplay()
    {
        await using var ctx = New();
        var applicationId = await IndApplicationAsync(ctx);
        var submission = await CreateAsync(ctx, applicationId, "IND read model");

        var detail = await new GetSubmissionHandler(ctx)
            .HandleAsync(submission.Id, CancellationToken.None);

        detail!.BoundTemplate.Should().NotBeNull();
        detail.BoundTemplate!.TemplateCode.Should().Be("FDA_IND_CTD");
        detail.BoundTemplate.VersionNumber.Should().BeGreaterThan(0);
        detail.BoundTemplate.TemplateVersionId
            .Should().Be(submission.BoundTemplateVersionId!.Value.Value);
    }

    [Fact]
    public async Task GetSubmission_ReportsNoBlueprintWhenUnbound()
    {
        await using var ctx = New();
        var applicationId = await DeviceApplicationAsync(ctx);
        var submission = await CreateAsync(ctx, applicationId, "510(k) read model");

        var detail = await new GetSubmissionHandler(ctx)
            .HandleAsync(submission.Id, CancellationToken.None);

        detail!.BoundTemplate.Should().BeNull();
    }

    [Fact]
    public async Task Create_BindsToThePublishedVersion_NeverADeprecatedOne()
    {
        await using var ctx = New();
        var applicationId = await IndApplicationAsync(ctx);

        var submission = await CreateAsync(ctx, applicationId, "IND not deprecated");

        var bound = await ctx.RegulatoryTemplates
            .AsNoTracking()
            .Include(t => t.Versions)
            .Where(t => t.Id == FdaIndCtd)
            .SelectMany(t => t.Versions)
            .SingleAsync(v => v.Id == submission.BoundTemplateVersionId!.Value);

        // EPIC-007a S002 superseded v1, which mislocated the Investigator's
        // Brochure; S004 superseded v2, which carried FDA's old wording and no
        // eCTD placement. Both were replaced rather than edited, and a new
        // submission must bind to neither — that is what deprecation is for.
        //
        // Asserted as "the only published one" rather than as a number, so the
        // next correction does not break a test about deprecation.
        bound.Status.Should().Be(TemplateVersionStatus.Published);

        var published = await ctx.RegulatoryTemplates
            .AsNoTracking()
            .Include(t => t.Versions)
            .Where(t => t.Id == FdaIndCtd)
            .SelectMany(t => t.Versions)
            .Where(v => v.Status == TemplateVersionStatus.Published)
            .ToListAsync();

        published.Should().ContainSingle()
            .Which.VersionNumber.Should().Be(bound.VersionNumber);
    }

    [Fact]
    public async Task ADeprecatedVersion_StaysIntactAndAttractsNothingNew()
    {
        await using var ctx = New();

        var deprecated = await ctx.RegulatoryTemplates
            .AsNoTracking()
            .Include(t => t.Versions)
                .ThenInclude(v => v.Sections)
            .Where(t => t.Id == FdaIndCtd)
            .SelectMany(t => t.Versions)
            .Where(v => v.Status == TemplateVersionStatus.Deprecated)
            .ToListAsync();

        deprecated.Should().NotBeEmpty(
            "S002 superseded v1 of the FDA IND blueprint rather than editing it");

        // Retained, not emptied or removed (ES-018). A filing made against this
        // version has to stay explicable, so its structure survives verbatim —
        // including the 1.13 that sent it here.
        deprecated.Should().AllSatisfy(v => v.Sections.Should().NotBeEmpty());
        deprecated.SelectMany(v => v.Sections)
            .Should().Contain(s => s.Code == "1.13"
                && s.Title == "Investigator's Brochure");

        // What deprecation actually stops.
        var applicationId = await IndApplicationAsync(ctx);
        var submission = await CreateAsync(ctx, applicationId, "IND post-deprecation");

        deprecated.Select(v => v.Id)
            .Should().NotContain(submission.BoundTemplateVersionId!.Value);
    }

    private async Task<SubmissionAggregate> CreateAsync(
        RegOSDbContext ctx,
        RegulatoryApplicationId applicationId,
        string title)
    {
        var handler = new CreateSubmissionHandler(ctx, new SubmissionRepository(ctx));

        var result = await handler.HandleAsync(
            new CreateSubmissionCommand(
                applicationId, title, SubmissionFormat.Ectd,
                TestSubmissionClassification.FdaApplication,
                TestSubmissionClassification.FdaOriginalApplication),
            CancellationToken.None);

        _submissionIds.Add(result.Id.Value);

        return await ctx.Submissions
            .AsNoTracking()
            .FirstAsync(s => s.Id == result.Id);
    }

    /// <summary>An FDA IND application — the CTD blueprint targets its type.</summary>
    private static async Task<RegulatoryApplicationId> IndApplicationAsync(
        RegOSDbContext ctx)
        => (await TestFdaApplication.EnsureAsync(ctx)).AppId;

    /// <summary>
    /// An FDA 510(k) application — same authority, and no blueprint targets its
    /// type. The authority-belonging rule is satisfied by construction now:
    /// RegulatoryApplication.Create would not have produced either of these
    /// applications with a type from another authority.
    /// </summary>
    private static async Task<RegulatoryApplicationId> DeviceApplicationAsync(
        RegOSDbContext ctx)
        => (await TestFdaApplication.Ensure510kAsync(ctx)).AppId;
}
