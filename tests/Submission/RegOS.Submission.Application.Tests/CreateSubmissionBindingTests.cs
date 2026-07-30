using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Commands.CreateSubmission;
using RegOS.Submission.Application.Queries.GetSubmission;
using RegOS.Submission.Application.Tests.Fixtures;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;

using ProductAggregate = RegOS.Product.Domain.Product.Product;
using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;
using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Application.Tests;

// Integration tests — exercise blueprint resolution against the real dev
// Postgres and its seeded reference data (docker postgres-local).
//
// The pair of cases is the point: two submission types under the SAME authority,
// one of which a published blueprint targets (FDA IND) and one of which none
// does (FDA 510(k)). That isolates "was a template found?" from "does the
// authority match?", which the create handler checks separately.
public sealed class CreateSubmissionBindingTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private static readonly AuthorityId Fda =
        new(Guid.Parse("20000000-0000-0000-0000-000000000001"));

    /// <summary>A pharma type the FDA IND (CTD) blueprint targets.</summary>
    private static readonly SubmissionTypeId FdaInd =
        new(Guid.Parse("40000000-0000-0000-0000-000000000008"));

    /// <summary>A device type under the same authority, with no blueprint.</summary>
    private static readonly SubmissionTypeId Fda510k =
        new(Guid.Parse("40000000-0000-0000-0000-000000000001"));

    private static readonly RegulatoryTemplateId FdaIndCtd =
        new(Guid.Parse("60000000-0000-0000-0000-000000000001"));

    private readonly List<Guid> _submissionIds = [];

    private static DbContextOptions<RegOSDbContext> Options() =>
        new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

    private static RegOSDbContext New() => new(Options(), TestTenant.Context);

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
        var applicationId = await EnsureFdaApplicationAsync(ctx);

        var submission = await CreateAsync(ctx, applicationId, FdaInd, "IND binding");

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
        var applicationId = await EnsureFdaApplicationAsync(ctx);

        var submission = await CreateAsync(ctx, applicationId, Fda510k, "510(k) binding");

        // Reference data that has no published blueprint must not block
        // creating a submission.
        submission.BoundTemplateVersionId.Should().BeNull();
    }

    [Fact]
    public async Task GetSubmission_ExposesTheBoundBlueprintForDisplay()
    {
        await using var ctx = New();
        var applicationId = await EnsureFdaApplicationAsync(ctx);
        var submission = await CreateAsync(ctx, applicationId, FdaInd, "IND read model");

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
        var applicationId = await EnsureFdaApplicationAsync(ctx);
        var submission = await CreateAsync(ctx, applicationId, Fda510k, "510(k) read model");

        var detail = await new GetSubmissionHandler(ctx)
            .HandleAsync(submission.Id, CancellationToken.None);

        detail!.BoundTemplate.Should().BeNull();
    }

    private async Task<SubmissionAggregate> CreateAsync(
        RegOSDbContext ctx,
        RegulatoryApplicationId applicationId,
        SubmissionTypeId submissionTypeId,
        string title)
    {
        var handler = new CreateSubmissionHandler(ctx, new SubmissionRepository(ctx));

        var result = await handler.HandleAsync(
            new CreateSubmissionCommand(applicationId, submissionTypeId, title),
            CancellationToken.None);

        _submissionIds.Add(result.Id.Value);

        return await ctx.Submissions
            .AsNoTracking()
            .FirstAsync(s => s.Id == result.Id);
    }

    /// <summary>
    /// A parent application pinned to the FDA, so the create handler's
    /// "submission type must belong to the application's authority" rule is
    /// satisfied for both submission types under test.
    /// </summary>
    private static async Task<RegulatoryApplicationId> EnsureFdaApplicationAsync(
        RegOSDbContext ctx)
        => (await TestFdaApplication.EnsureAsync(ctx)).AppId;
}
