using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using RegOS.Interaction.Domain.Commitments;
using RegOS.Interaction.Domain.Correspondence;
using RegOS.Interaction.Domain.Inspections;
using RegOS.Interaction.Domain.Meetings;
using RegOS.Persistence;
using RegOS.Process.Application.Commands.InstantiateProcessPlan;
using RegOS.Process.Application.Queries.GetProcessPlan;
using RegOS.Process.Application.Tests.Fixtures;
using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.Process.Infrastructure.Repositories;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.Regulatory.Correspondence;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Application.Tests;

/// <summary>
/// <b>The pre-IND track, finally attachable.</b> S006 gave the playbook's
/// output half — <c>SUBMIT</c> and the registration that follows it — somewhere
/// to point. Its first half was unreachable: <c>PRE-IND-REQ</c> is a letter,
/// <c>PRE-IND-MTG</c> is a meeting and <c>PRE-IND-MIN</c> is a letter, and none
/// of those could say which step they served.
/// </summary>
/// <remarks>
/// The scheduling tests found that this track — not the 150-day CMC package —
/// is the seeded playbook's critical path. <b>These tests are that finding
/// made operable</b>: the steps a team would hurry are now the steps whose real
/// artefacts are discoverable from the plan.
/// <para>
/// Every assertion is still ADR-065 <b>I9</b>. Four more aggregates, and the
/// success criterion is that nothing interesting happens.
/// </para>
/// </remarks>
[Collection(ProcessDatabase.Collection)]
public sealed class InteractionAttachmentTests
{
    private static readonly CountryId UnitedStates =
        new(Guid.Parse("10000000-0000-0000-0000-000000000001"));

    private static readonly AuthorityId Fda =
        new(Guid.Parse("20000000-0000-0000-0000-000000000001"));

    private static readonly CorrespondenceTypeId MeetingRequest =
        new(Guid.Parse("90000000-0000-0000-0000-000000000005"));

    private static readonly CorrespondenceTypeId MeetingMinutes =
        new(Guid.Parse("90000000-0000-0000-0000-000000000006"));

    private static readonly DateOnly Anchor = new(2026, 9, 1);

    private readonly ProcessDatabase _database;

    public InteractionAttachmentTests(ProcessDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// All four kinds, on the four steps a real IND team would put them on, read
    /// back from the plan in one query.
    /// </summary>
    [Fact]
    public async Task The_pre_IND_track_shows_the_work_that_served_it()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await AnIndPlan(context);
        var step = await StepsByCode(context, plan);

        var request = ALetter(context, MeetingRequest, "Type B meeting request");
        var minutes = ALetter(context, MeetingMinutes, "Type B meeting minutes");
        var meeting = AMeeting(context, "Pre-IND Type B meeting");
        var safety = ACommitment(context, "30-day safety reporting");

        await context.SaveChangesAsync();

        await Attach(request, step["PRE-IND-REQ"]);
        await Attach(meeting, step["PRE-IND-MTG"]);
        await Attach(minutes, step["PRE-IND-MIN"]);
        await Attach(safety, step["SAFETY-30"]);

        await using var reread = _database.NewContext(TestTenant.Context);
        var details = await Read(reread, plan);

        Attached(details, "PRE-IND-REQ").Should().ContainSingle()
            .Which.Kind.Should().Be("Correspondence");

        Attached(details, "PRE-IND-MTG").Should().ContainSingle()
            .Which.Kind.Should().Be("Meeting");

        Attached(details, "PRE-IND-MIN").Should().ContainSingle()
            .Which.Title.Should().Be("Type B meeting minutes");

        Attached(details, "SAFETY-30").Should().ContainSingle()
            .Which.Kind.Should().Be("Commitment");

        // And the other eight steps are neither incomplete nor invalid (I9).
        details.Steps
            .Where(x => x.Attached.Count == 0)
            .Should().HaveCount(8);
    }

    /// <summary>
    /// <b>I9 for the aggregate that has the most to lose.</b> A meeting carries
    /// a lifecycle, minutes and an outcome; attaching touches none of them.
    /// </summary>
    [Fact]
    public async Task Attaching_changes_nothing_about_the_meeting()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await AnIndPlan(context);
        var step = await StepsByCode(context, plan);

        var meeting = AMeeting(context, "Pre-IND Type B meeting");

        meeting.ChangeStatus(HaMeetingStatus.Held, new DateOnly(2026, 10, 1));
        meeting.RecordOutcome("Discussed the Phase 1 design.", "Agency accepted it.");

        await context.SaveChangesAsync();

        var before = await MeetingSnapshot(meeting.Id);

        await Attach(meeting, step["PRE-IND-MTG"]);

        (await MeetingSnapshot(meeting.Id)).Should().BeEquivalentTo(before,
            because: "a meeting attached to a step is the meeting it already was "
                + "— the step does not hold it and holding it does not complete "
                + "the step");
    }

    /// <summary>
    /// <b>The ADR-042 question, answered as a test.</b> A step is not a fourth
    /// business origin.
    /// </summary>
    /// <remarks>
    /// ADR-042 decision 2 fires on a fourth independent <em>origin</em> — where a
    /// commitment arose. A step is what a commitment <em>serves</em>. The clause
    /// was deliberately worded as origins rather than columns; this is the first
    /// thing to test that wording, and both halves hold: a commitment born in a
    /// letter keeps exactly that origin, and one born nowhere gains none.
    /// </remarks>
    [Fact]
    public async Task A_step_is_not_a_fourth_origin()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await AnIndPlan(context);
        var step = await StepsByCode(context, plan);

        var letter = ALetter(context, MeetingMinutes, "Approval letter");
        await context.SaveChangesAsync();

        var fromALetter = ACommitment(context, "Stability data by Q3", letter.Id);
        var fromNowhere = ACommitment(context, "Annual report");

        await context.SaveChangesAsync();

        await Attach(fromALetter, step["SAFETY-30"]);
        await Attach(fromNowhere, step["SAFETY-30"]);

        await using var reread = _database.NewContext(TestTenant.Context);

        var origins = await reread.Commitments
            .AsNoTracking()
            .Where(x => x.Id == fromALetter.Id || x.Id == fromNowhere.Id)
            .Select(x => new
            {
                x.Id,
                x.SourceCorrespondenceId,
                x.SourceMeetingId,
                x.SourceInspectionId,
                x.ProcessStepId
            })
            .ToListAsync();

        var born = origins.Single(x => x.Id == fromALetter.Id);
        born.SourceCorrespondenceId.Should().Be(letter.Id);
        born.SourceMeetingId.Should().BeNull();
        born.SourceInspectionId.Should().BeNull();
        born.ProcessStepId.Should().NotBeNull(
            "it serves a step, which is a different fact from where it arose");

        var unborn = origins.Single(x => x.Id == fromNowhere.Id);
        unborn.SourceCorrespondenceId.Should().BeNull();
        unborn.SourceMeetingId.Should().BeNull();
        unborn.SourceInspectionId.Should().BeNull(
            "attaching a commitment to planned work gives it no origin it "
                + "did not have — ADR-042's clause counts origins, not columns");
    }

    /// <summary>
    /// The reverse, for all four at once: the plan's schedule is untouched by
    /// anything attaching to it. Process gains readers, never owners.
    /// </summary>
    [Fact]
    public async Task Attaching_changes_nothing_about_the_plan()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await AnIndPlan(context);
        var step = await StepsByCode(context, plan);

        var before = Schedule(await Read(context, plan));

        var letter = ALetter(context, MeetingRequest, "Meeting request");
        var meeting = AMeeting(context, "Pre-IND meeting");
        var inspection = AnInspection(context, "Pre-approval inspection");
        var commitment = ACommitment(context, "Safety reporting");

        await context.SaveChangesAsync();

        await Attach(letter, step["PRE-IND-REQ"]);
        await Attach(meeting, step["PRE-IND-MTG"]);
        await Attach(inspection, step["SUBMIT"]);
        await Attach(commitment, step["SAFETY-30"]);

        await using var reread = _database.NewContext(TestTenant.Context);

        Schedule(await Read(reread, plan)).Should().BeEquivalentTo(before,
            because: "four annotations arrived and not one date moved");
    }

    /// <summary>Clearing is always permitted, on every one of the four.</summary>
    [Fact]
    public async Task Detaching_leaves_both_ends_as_they_were()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await AnIndPlan(context);
        var step = await StepsByCode(context, plan);

        var inspection = AnInspection(context, "Pre-approval inspection");
        await context.SaveChangesAsync();

        var before = await InspectionSnapshot(inspection.Id);

        await Attach(inspection, step["SUBMIT"]);
        await Attach(inspection, null);

        (await InspectionSnapshot(inspection.Id)).Should().BeEquivalentTo(before);

        await using var reread = _database.NewContext(TestTenant.Context);
        (await Read(reread, plan)).Steps
            .Should().OnlyContain(x => x.Attached.Count == 0);
    }

    // --- fixtures ------------------------------------------------------------

    private static IReadOnlyList<AttachedArtefact> Attached(
        ProcessPlanDetails plan, string code)
        => plan.Steps.Single(x => x.Code == code).Attached;

    private static object Schedule(ProcessPlanDetails plan)
        => plan.Steps
            .Select(x => new { x.Code, x.PlannedStartOn, x.PlannedEndOn, x.Status })
            .ToList();

    private static Task<ProcessPlanDetails> Read(
        RegOSDbContext context, ProcessPlanId id)
        => new GetProcessPlanHandler(context)
            .HandleAsync(new GetProcessPlanQuery(id.Value));

    private async Task<Dictionary<string, ProcessStepId>> StepsByCode(
        RegOSDbContext context, ProcessPlanId planId)
    {
        var details = await Read(context, planId);

        return details.Steps.ToDictionary(
            x => x.Code, x => ProcessStepId.From(x.Id));
    }

    /// <summary>
    /// Attaching goes through the owning aggregate, exactly as the handlers do.
    /// <b>There is no path here that writes through a Process repository</b> —
    /// there is no such path to write.
    /// </summary>
    private async Task Attach(HaCorrespondence letter, ProcessStepId? step)
        => await Save(async context =>
            (await context.HaCorrespondence.FirstAsync(x => x.Id == letter.Id))
                .AttachToStep(step));

    private async Task Attach(HaMeeting meeting, ProcessStepId? step)
        => await Save(async context =>
            (await context.HaMeetings.FirstAsync(x => x.Id == meeting.Id))
                .AttachToStep(step));

    private async Task Attach(Inspection inspection, ProcessStepId? step)
        => await Save(async context =>
            (await context.Inspections.FirstAsync(x => x.Id == inspection.Id))
                .AttachToStep(step));

    private async Task Attach(Commitment commitment, ProcessStepId? step)
        => await Save(async context =>
            (await context.Commitments.FirstAsync(x => x.Id == commitment.Id))
                .AttachToStep(step));

    private async Task Save(Func<RegOSDbContext, Task> mutate)
    {
        await using var context = _database.NewContext(TestTenant.Context);

        await mutate(context);
        await context.SaveChangesAsync();
    }

    private async Task<object> MeetingSnapshot(HaMeetingId id)
    {
        await using var context = _database.NewContext(TestTenant.Context);

        return await context.HaMeetings
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => (object)new
            {
                x.CurrentStatus,
                x.Subject,
                x.ScheduledFor,
                x.Minutes,
                x.Outcome,
                x.AuthorityId,
                HistoryCount = x.History.Count
            })
            .FirstAsync();
    }

    private async Task<object> InspectionSnapshot(InspectionId id)
    {
        await using var context = _database.NewContext(TestTenant.Context);

        return await context.Inspections
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => (object)new
            {
                x.CurrentStatus,
                x.Title,
                x.ScheduledFor,
                x.Outcome,
                x.OrganizationSiteId,
                HistoryCount = x.History.Count
            })
            .FirstAsync();
    }

    private static HaCorrespondence ALetter(
        RegOSDbContext context, CorrespondenceTypeId type, string subject)
    {
        var letter = HaCorrespondence.Record(
            TestTenant.Id, Fda, type, null,
            CorrespondenceDirection.Outbound, subject, new DateOnly(2026, 9, 5));

        context.HaCorrespondence.Add(letter);

        return letter;
    }

    private static HaMeeting AMeeting(RegOSDbContext context, string subject)
    {
        var meeting = HaMeeting.Begin(
            TestTenant.Id, Fda, subject,
            HaMeetingStatus.Granted, new DateOnly(2026, 9, 5),
            scheduledFor: new DateOnly(2026, 10, 1));

        context.HaMeetings.Add(meeting);

        return meeting;
    }

    private static Inspection AnInspection(RegOSDbContext context, string title)
    {
        var inspection = Inspection.Begin(
            TestTenant.Id, Fda, title,
            InspectionStatus.Announced, new DateOnly(2026, 9, 5));

        context.Inspections.Add(inspection);

        return inspection;
    }

    private static Commitment ACommitment(
        RegOSDbContext context,
        string title,
        HaCorrespondenceId? source = null)
    {
        var commitment = Commitment.Give(
            TestTenant.Id, Fda, title,
            new DateOnly(2026, 9, 5), new DateOnly(2027, 3, 1),
            sourceCorrespondenceId: source);

        context.Commitments.Add(commitment);

        return commitment;
    }

    /// <summary>
    /// A plan off the <b>seeded</b> US·FDA·IND playbook — the one whose twelve
    /// steps the scheduling tests measured. Read, never mutated: a test owns the
    /// data it writes to, and this is shared.
    /// </summary>
    private async Task<ProcessPlanId> AnIndPlan(RegOSDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var product = GlobalProduct.Register(
            TestTenant.Id, $"INT-{suffix}", $"Interaction fixture {suffix}",
            ProductType.Drug);

        context.Products.Add(product);

        var objective = ProcessObjective.Create(
            TestTenant.Id, product.Id, UnitedStates, "Open an IND", Anchor);

        context.ProcessObjectives.Add(objective);

        await context.SaveChangesAsync();

        var versionId = await context.ProcessDefinitions
            .AsNoTracking()
            .Where(x => x.Code == "US-FDA-IND-INITIAL")
            .SelectMany(x => x.Versions)
            .Where(v => v.Status == ProcessDefinitionVersionStatus.Published)
            .OrderBy(v => v.VersionNumber)
            .Select(v => v.Id)
            .FirstAsync();

        await using var scope = _database.NewContext(TestTenant.Context);

        var result = await new InstantiateProcessPlanHandler(
                new ProcessPlanRepository(scope),
                new ProcessDefinitionRepository(scope),
                scope,
                new FixedTenant())
            .HandleAsync(
                new InstantiateProcessPlanCommand(
                    objective.Id, versionId, Anchor, "US IND filing plan", Anchor),
                CancellationToken.None);

        return ProcessPlanId.From(result.Id);
    }

    private sealed class FixedTenant : ITenantContext
    {
        public TenantId TenantId => TestTenant.Id;

        public TenantId? TenantIdOrNull => TestTenant.Id;
    }
}
