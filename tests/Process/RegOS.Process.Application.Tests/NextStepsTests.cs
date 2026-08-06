using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Application.Queries.ListNextSteps;
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
/// <em>"What can I work on today?"</em> — the first operational read in the
/// Process context.
/// </summary>
/// <remarks>
/// Every test supplies its own <c>asOf</c>, which is the point: lateness is
/// judged against a date the caller gives, so the answer is reproducible rather
/// than dependent on when the suite happened to run.
/// </remarks>
[Collection(ProcessDatabase.Collection)]
public sealed class NextStepsTests
{
    private static readonly CountryId UnitedStates =
        new(Guid.Parse("10000000-0000-0000-0000-000000000001"));

    private static readonly DateOnly Anchor = new(2026, 9, 1);

    private readonly ProcessDatabase _database;

    public NextStepsTests(ProcessDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// A draft plan has not been committed to, so it generates no work.
    /// </summary>
    [Fact]
    public async Task A_draft_plan_contributes_nothing()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await APlanAsync(context, activate: false);

        var items = await Read(context, Anchor);

        items.Should().NotContain(x => x.PlanId == plan.Id.Value);
    }

    /// <summary>
    /// <b>The readiness flag, and what it deliberately is not.</b> A step whose
    /// predecessor is unsettled is not ready — and nothing anywhere completes it.
    /// </summary>
    [Fact]
    public async Task A_step_is_ready_only_when_its_predecessors_are_settled()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await APlanAsync(context);

        var before = Mine(await Read(context, Anchor), plan);

        before.Single(x => x.Code == "A").IsReady.Should().BeTrue(
            "nothing precedes the first step");

        var second = before.Single(x => x.Code == "B");
        second.IsReady.Should().BeFalse();
        second.WaitingOn.Should().BeEquivalentTo(["A"]);

        // Complete A, and B becomes ready — but stays NotStarted.
        await Complete(plan, "A", Anchor.AddDays(4));

        await using var reread = _database.NewContext(TestTenant.Context);
        var after = Mine(await Read(reread, Anchor), plan);

        var ready = after.Single(x => x.Code == "B");
        ready.IsReady.Should().BeTrue();
        ready.Status.Should().Be(nameof(ProcessStepStatus.NotStarted),
            "readiness is a fact about the schedule, never a transition");
    }

    /// <summary>
    /// A skipped predecessor unblocks its successors exactly as a completed one
    /// does — in both cases nothing further is expected of it.
    /// </summary>
    [Fact]
    public async Task A_skipped_predecessor_unblocks_what_follows_it()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await APlanAsync(context);

        await Skip(plan, "A", Anchor.AddDays(2), "Not applicable to this route.");

        await using var reread = _database.NewContext(TestTenant.Context);

        Mine(await Read(reread, Anchor), plan)
            .Single(x => x.Code == "B").IsReady.Should().BeTrue();
    }

    /// <summary>
    /// Lateness is judged against the query's date, not a clock — so the same
    /// plan is on time on one date and late on another, deterministically.
    /// </summary>
    [Fact]
    public async Task Lateness_is_measured_against_the_date_the_caller_supplies()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await APlanAsync(context);

        var onTime = Mine(await Read(context, Anchor), plan);
        onTime.Should().OnlyContain(x => x.DaysLate == null);

        // Step A is planned 1–5 September. Ask on the 8th.
        var late = Mine(await Read(context, Anchor.AddDays(7)), plan);

        late.Single(x => x.Code == "A").DaysLate.Should().Be(3);
    }

    /// <summary>Settled steps leave the board; that is what makes it a board.</summary>
    [Fact]
    public async Task A_completed_step_disappears_from_the_list()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await APlanAsync(context);

        await Complete(plan, "A", Anchor.AddDays(4));

        await using var reread = _database.NewContext(TestTenant.Context);

        Mine(await Read(reread, Anchor), plan)
            .Should().NotContain(x => x.Code == "A");
    }

    /// <summary>
    /// <b>D7.</b> The plan board is a sibling of the due-work view, not a
    /// replacement — an obligation a regulator is waiting on and our own plan
    /// slipping are two facts, and this query answers only the second.
    /// </summary>
    [Fact]
    public async Task The_board_reports_only_plan_work()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        await APlanAsync(context);

        var items = await Read(context, Anchor);

        items.Should().NotBeEmpty();
        items.Should().OnlyContain(x => x.PlanId != Guid.Empty,
            "every row here belongs to a plan; obligations live in ListDueWork");
    }

    // --- fixtures ------------------------------------------------------------

    private static Task<IReadOnlyList<NextStepItem>> Read(
        RegOSDbContext context, DateOnly asOf)
        => new ListNextStepsHandler(context)
            .HandleAsync(new ListNextStepsQuery(asOf));

    private static List<NextStepItem> Mine(
        IReadOnlyList<NextStepItem> items, ProcessPlan plan)
        => items.Where(x => x.PlanId == plan.Id.Value).ToList();

    private async Task Complete(ProcessPlan plan, string code, DateOnly on)
        => await Mutate(plan, code, (p, id) => p.CompleteStep(id, on));

    private async Task Skip(
        ProcessPlan plan, string code, DateOnly on, string reason)
        => await Mutate(plan, code, (p, id) => p.SkipStep(id, on, reason));

    private async Task Mutate(
        ProcessPlan plan, string code, Action<ProcessPlan, ProcessStepId> act)
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var tracked = await context.ProcessPlans
            .Include(x => x.Steps)
            .FirstAsync(x => x.Id == plan.Id);

        act(tracked, tracked.Steps.Single(x => x.Code == code).Id);

        await context.SaveChangesAsync();
    }

    private static async Task<ProcessPlan> APlanAsync(
        RegOSDbContext context, bool activate = true)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var product = GlobalProduct.Register(
            TestTenant.Id, $"NEXT-{suffix}", $"Next fixture {suffix}",
            ProductType.Drug);

        context.Products.Add(product);

        var objective = ProcessObjective.Create(
            TestTenant.Id, product.Id, UnitedStates, "Open an IND", Anchor);

        context.ProcessObjectives.Add(objective);

        var definition = ProcessDefinition.Create(
            $"NEXT-{suffix}",
            $"Next playbook {suffix}",
            UnitedStates,
            new AuthorityId(Guid.Parse("20000000-0000-0000-0000-000000000001")),
            new ApplicationTypeId(Guid.Parse("40000000-0000-0000-0000-000000000008")),
            DateTime.UtcNow,
            tenantId: TestTenant.Id);

        var version = definition.StartDraftVersion();
        var first = definition.AddStep("A", "First", durationDays: 5);
        var second = definition.AddStep("B", "Second", durationDays: 3);
        definition.AddStepPredecessor(second.Id, first.Id);
        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        context.ProcessDefinitions.Add(definition);

        var plan = ProcessPlan.InstantiateFrom(
            TestTenant.Id, objective.Id, version, Anchor, "Filing plan", Anchor);

        if (activate) plan.Activate(Anchor);

        context.ProcessPlans.Add(plan);

        await context.SaveChangesAsync();

        return plan;
    }
}
