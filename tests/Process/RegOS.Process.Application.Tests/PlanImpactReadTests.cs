using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Application.Queries.GetPlanImpact;
using RegOS.Process.Application.Tests.Fixtures;
using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;

namespace RegOS.Process.Application.Tests;

/// <summary>
/// The impact read, through the database — <b>and the property that keeps it
/// safe</b>.
/// </summary>
[Collection(ProcessDatabase.Collection)]
public sealed class PlanImpactReadTests
{
    private static readonly CountryId UnitedStates =
        new(Guid.Parse("10000000-0000-0000-0000-000000000001"));

    private static readonly DateOnly Anchor = new(2026, 9, 1);

    private readonly ProcessDatabase _database;

    public PlanImpactReadTests(ProcessDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// <b>ADR-065 I8, asserted rather than asserted about.</b> The plan's own
    /// dates are byte-identical after the analysis runs — a projection is
    /// computed, returned and discarded.
    /// </summary>
    [Fact]
    public async Task Running_the_analysis_changes_nothing_about_the_plan()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await APlanAsync(context);

        var before = await Dates(plan.Id);

        await Read(context, plan.Id, Anchor.AddDays(60));
        await Read(context, plan.Id, Anchor.AddDays(120));

        var after = await Dates(plan.Id);

        after.Should().BeEquivalentTo(before,
            "the forecast is not the ledger — nothing is persisted, and the "
                + "plan keeps the dates it was scheduled with");
    }

    /// <summary>
    /// The same question always gets the same answer — I5's property, applied to
    /// a read.
    /// </summary>
    [Fact]
    public async Task The_same_question_gives_the_same_answer()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await APlanAsync(context);

        var first = await Read(context, plan.Id, Anchor.AddDays(30));
        var second = await Read(context, plan.Id, Anchor.AddDays(30));

        second.Should().BeEquivalentTo(first);
    }

    /// <summary>Nothing has slipped: no late steps, no movement.</summary>
    [Fact]
    public async Task A_plan_on_schedule_reports_no_impact()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await APlanAsync(context);

        var impact = await Read(context, plan.Id, Anchor);

        impact.LateSteps.Should().BeEmpty();
        impact.SlipDays.Should().Be(0);
        impact.ProjectedFinishOn.Should().Be(impact.PlannedFinishOn);
    }

    /// <summary>
    /// <b>The management answer.</b> QUICK is late and has slack; the finish does
    /// not move, and the read says so while still reporting the late step.
    /// </summary>
    [Fact]
    public async Task A_late_step_inside_slack_is_reported_without_moving_the_finish()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await APlanAsync(context, startSlow: true);

        // QUICK was due 2 Sep. Ask on the 11th.
        var impact = await Read(context, plan.Id, new DateOnly(2026, 9, 11));

        var late = impact.LateSteps.Should().ContainSingle().Subject;
        late.Code.Should().Be("QUICK");
        late.DaysLate.Should().Be(9);

        impact.SlipDays.Should().Be(0,
            "nine days late inside thirty-eight days of slack costs nothing");
    }

    /// <summary>
    /// Affected follows topology; actionable is the subset still open. A settled
    /// step downstream of a delay is affected and not actionable.
    /// </summary>
    [Fact]
    public async Task Affected_and_actionable_are_reported_separately()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await APlanAsync(context, startSlow: true);

        var impact = await Read(context, plan.Id, new DateOnly(2026, 9, 11));

        var affected = impact.LateSteps.Single().Affected;

        affected.Should().ContainSingle(x => x.Code == "JOIN");
        affected.Single().IsActionable.Should().BeTrue();
    }

    /// <summary>A settled step that finished late is history, not a risk.</summary>
    [Fact]
    public async Task A_completed_step_is_never_reported_as_late()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await APlanAsync(context, startSlow: true);

        await using var mutate = _database.NewContext(TestTenant.Context);
        var tracked = await mutate.ProcessPlans
            .Include(x => x.Steps)
            .FirstAsync(x => x.Id == plan.Id);

        tracked.CompleteStep(
            tracked.Steps.Single(x => x.Code == "QUICK").Id,
            new DateOnly(2026, 9, 11));

        await mutate.SaveChangesAsync();

        await using var reread = _database.NewContext(TestTenant.Context);
        var impact = await Read(reread, plan.Id, new DateOnly(2026, 9, 11));

        impact.LateSteps.Should().NotContain(x => x.Code == "QUICK",
            "the question is what still threatens the finish, not what once did");
    }

    // --- fixtures ------------------------------------------------------------

    private static Task<PlanImpactDetails> Read(
        RegOSDbContext context, ProcessPlanId id, DateOnly asOf)
        => new GetPlanImpactHandler(context)
            .HandleAsync(new GetPlanImpactQuery(id.Value, asOf));

    private async Task<List<object>> Dates(ProcessPlanId id)
    {
        await using var context = _database.NewContext(TestTenant.Context);

        return await context.ProcessPlans
            .AsNoTracking()
            .Where(x => x.Id == id)
            .SelectMany(x => x.Steps)
            .OrderBy(x => x.Code)
            .Select(x => (object)new
            {
                x.Code,
                x.PlannedStartOn,
                x.PlannedEndOn
            })
            .ToListAsync();
    }

    private static async Task<ProcessPlan> APlanAsync(
        RegOSDbContext context, bool startSlow = false)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var product = GlobalProduct.Register(
            TestTenant.Id, $"IMP-{suffix}", $"Impact fixture {suffix}",
            ProductType.Drug);

        context.Products.Add(product);

        var objective = ProcessObjective.Create(
            TestTenant.Id, product.Id, UnitedStates, "Open an IND", Anchor);

        context.ProcessObjectives.Add(objective);

        var definition = ProcessDefinition.Create(
            $"IMP-{suffix}",
            $"Impact playbook {suffix}",
            UnitedStates,
            new AuthorityId(Guid.Parse("20000000-0000-0000-0000-000000000001")),
            new ApplicationTypeId(Guid.Parse("40000000-0000-0000-0000-000000000008")),
            DateTime.UtcNow,
            tenantId: TestTenant.Id);

        var version = definition.StartDraftVersion();
        var quick = definition.AddStep("QUICK", "Quick", durationDays: 2);
        var slow = definition.AddStep("SLOW", "Slow", durationDays: 40);
        var join = definition.AddStep("JOIN", "Join", durationDays: 1);
        definition.AddStepPredecessor(join.Id, quick.Id);
        definition.AddStepPredecessor(join.Id, slow.Id);
        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        context.ProcessDefinitions.Add(definition);

        var plan = ProcessPlan.InstantiateFrom(
            TestTenant.Id, objective.Id, version, Anchor, "Filing plan", Anchor);

        plan.Activate(Anchor);

        // Without this, SLOW is "not started" and therefore late too — correct
        // behaviour, and it would drown the case these tests are about.
        if (startSlow)
            plan.StartStep(plan.Steps.Single(x => x.Code == "SLOW").Id, Anchor);

        context.ProcessPlans.Add(plan);

        await context.SaveChangesAsync();

        return plan;
    }
}
