using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Application.Commands.InstantiateProcessPlan;
using RegOS.Process.Application.Queries.GetProcessPlan;
using RegOS.Process.Application.Tests.Fixtures;
using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.Process.Infrastructure.Repositories;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Application.Tests;

/// <summary>
/// Instantiating the <b>seeded US·FDA·IND playbook</b> — twelve real steps,
/// three converging strands — and reading the plan back.
/// </summary>
/// <remarks>
/// The domain tests prove the arithmetic without a database. This proves the two
/// things they cannot: that the derived schedule <b>survives a round trip through
/// Postgres</b> with its graph intact, and that the read composes the plan, the
/// objective and the playbook version it is pinned to.
/// </remarks>
[Collection(ProcessDatabase.Collection)]
public sealed class ProcessPlanInstantiationTests
{
    private static readonly CountryId UnitedStates =
        new(Guid.Parse("10000000-0000-0000-0000-000000000001"));

    private static readonly DateOnly Anchor = new(2026, 9, 1);

    private readonly ProcessDatabase _database;

    public ProcessPlanInstantiationTests(ProcessDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task The_seeded_playbook_instantiates_into_twelve_dated_steps()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var plan = await Instantiate(context);
        var details = await Read(context, plan.Id);

        details.Steps.Should().HaveCount(12);
        details.AnchorDate.Should().Be(Anchor);
        details.Status.Should().Be("Draft");

        // Every step is dated, and no step ends before it starts.
        details.Steps.Should().OnlyContain(
            step => step.PlannedEndOn >= step.PlannedStartOn);
    }

    /// <summary>
    /// The step with no predecessors starts on the anchor itself, and the plan's
    /// span is derived from its steps rather than stored beside them.
    /// </summary>
    [Fact]
    public async Task The_schedule_starts_at_the_anchor_and_runs_to_the_last_step()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var details = await Read(context, (await Instantiate(context)).Id);

        details.Steps.Single(x => x.Code == "PRE-IND-REQ")
            .PlannedStartOn.Should().Be(Anchor);

        details.PlannedStartOn.Should().Be(Anchor);
        details.PlannedEndOn.Should().Be(
            details.Steps.Max(x => x.PlannedEndOn));
    }

    /// <summary>
    /// <b>The convergence, on the real playbook — and it does not run where you
    /// would guess.</b>
    /// </summary>
    /// <remarks>
    /// <c>COMPILE</c> waits for <c>CMC</c>, <c>IB</c> and <c>FORMS</c>, and the
    /// obvious assumption is that the 150-day CMC package is what holds it up.
    /// <b>It is not.</b> The critical path runs through the pre-IND meeting
    /// track — request, package, meeting, minutes, protocol, forms — because
    /// FDA's calendar contributes 30 days twice before the protocol can even be
    /// written.
    /// <para>
    /// <b>This test was written asserting CMC and was wrong by 33 days.</b> Kept
    /// in this shape because it is exactly what a scheduling model is <em>for</em>:
    /// the answer was not derivable by looking at the largest number, and a
    /// regulatory team guessing from the durations alone would plan the wrong
    /// thing to hurry.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task Compilation_waits_for_the_latest_strand_which_is_the_meeting_track()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var details = await Read(context, (await Instantiate(context)).Id);

        var compile = details.Steps.Single(x => x.Code == "COMPILE");

        compile.Predecessors.Should().BeEquivalentTo(["CMC", "FORMS", "IB"]);

        var latest = details.Steps
            .Where(x => compile.Predecessors.Contains(x.Code))
            .Max(x => x.PlannedEndOn);

        compile.PlannedStartOn.Should().Be(latest.AddDays(1),
            "a converging step starts the day after the LAST thing it waits for");

        // And the strand that decides it is FORMS, not the 150-day CMC package.
        details.Steps.Single(x => x.Code == "FORMS")
            .PlannedEndOn.Should().Be(latest);

        details.Steps.Single(x => x.Code == "CMC")
            .PlannedEndOn.Should().BeBefore(latest,
                "the science finishes before the regulatory calendar does");
    }

    /// <summary>
    /// <b>ADR-065 I5, through the database.</b> Two plans from the same version,
    /// objective and anchor carry identical schedules — different ids, same
    /// answer.
    /// </summary>
    [Fact]
    public async Task Instantiating_twice_produces_the_same_schedule()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var first = await Read(context, (await Instantiate(context)).Id);
        var second = await Read(context, (await Instantiate(context)).Id);

        Shape(second).Should().BeEquivalentTo(Shape(first));

        first.Id.Should().NotBe(second.Id,
            "two attempts at one objective are two plans");
    }

    /// <summary>
    /// <b>I4, and D6's disclosure.</b> Publishing a newer version of the playbook
    /// changes nothing about a plan already pinned to the old one — and the plan
    /// says the version has moved on rather than quietly migrating.
    /// </summary>
    /// <remarks>
    /// <b>It authors its own playbook rather than superseding the seeded one.</b>
    /// The first draft of this test mutated <c>US-FDA-IND-INITIAL</c> and broke
    /// five S001 tests that read it — a test owns the data it mutates, and a
    /// shared seed is the one thing it must never write to.
    /// </remarks>
    [Fact]
    public async Task Superseding_the_version_leaves_the_plan_untouched_and_says_so()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var definition = await AnOwnDefinitionAsync(context);
        var firstVersion = definition.Versions.Single().Id;

        var objective = await AnObjectiveAsync(context);
        var plan = await InstantiateFrom(objective, firstVersion);

        var before = await Read(context, plan.Id);

        before.DefinitionVersionIsSuperseded.Should().BeFalse();

        // A second version of this playbook, published, superseding the first.
        await using var authoring = _database.NewContext(TestTenant.Context);
        var definitions = new ProcessDefinitionRepository(authoring);

        var tracked = await definitions.GetByIdAsync(
            definition.Id, CancellationToken.None);

        var next = tracked!.StartDraftVersion();
        tracked.AddStep("REPLACEMENT", "A different process");
        tracked.PublishVersion(next.Id, new DateOnly(2027, 1, 1), DateTime.UtcNow);
        tracked.SupersedeVersion(firstVersion);

        await definitions.UpdateAsync(tracked, CancellationToken.None);

        await using var reading = _database.NewContext(TestTenant.Context);
        var after = await Read(reading, plan.Id);

        after.Steps.Should().BeEquivalentTo(before.Steps,
            because: "a pinned plan is a historical record, not a projection");
        after.ProcessDefinitionVersionId.Should().Be(
            before.ProcessDefinitionVersionId);
        after.DefinitionVersionIsSuperseded.Should().BeTrue(
            "the plan discloses that the playbook moved on; it does not migrate");
    }

    // --- fixtures ------------------------------------------------------------

    private static object Shape(ProcessPlanDetails plan)
        => plan.Steps
            .Select(step => new
            {
                step.Code,
                step.PlannedStartOn,
                step.PlannedEndOn,
                step.Predecessors
            })
            .ToList();

    private static Task<ProcessPlanDetails> Read(
        RegOSDbContext context, Guid id)
        => new GetProcessPlanHandler(context)
            .HandleAsync(new GetProcessPlanQuery(id));

    private static async Task<ProcessDefinitionId> SeededDefinitionId(
        RegOSDbContext context)
        => await context.ProcessDefinitions
            .AsNoTracking()
            .Where(x => x.Code == "US-FDA-IND-INITIAL")
            .Select(x => x.Id)
            .FirstAsync();

    private async Task<InstantiateProcessPlanResult> Instantiate(
        RegOSDbContext context)
    {
        var objective = await AnObjectiveAsync(context);

        var versionId = await context.ProcessDefinitions
            .AsNoTracking()
            .Where(x => x.Code == "US-FDA-IND-INITIAL")
            .SelectMany(x => x.Versions)
            .Where(v => v.Status == ProcessDefinitionVersionStatus.Published)
            .OrderBy(v => v.VersionNumber)
            .Select(v => v.Id)
            .FirstAsync();

        return await InstantiateFrom(objective, versionId);
    }

    private async Task<InstantiateProcessPlanResult> InstantiateFrom(
        ProcessObjectiveId objective, ProcessDefinitionVersionId versionId)
    {
        await using var scope = _database.NewContext(TestTenant.Context);

        var handler = new InstantiateProcessPlanHandler(
            new ProcessPlanRepository(scope),
            new ProcessDefinitionRepository(scope),
            scope,
            new FixedTenant());

        return await handler.HandleAsync(
            new InstantiateProcessPlanCommand(
                objective, versionId, Anchor, "US IND filing plan", Anchor),
            CancellationToken.None);
    }

    /// <summary>
    /// A playbook this test class owns outright, so mutating it cannot disturb
    /// the seeded one every other test reads.
    /// </summary>
    private static async Task<ProcessDefinition> AnOwnDefinitionAsync(
        RegOSDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var definition = ProcessDefinition.Create(
            $"OWNED-{suffix}",
            $"Owned playbook {suffix}",
            UnitedStates,
            new AuthorityId(Guid.Parse("20000000-0000-0000-0000-000000000001")),
            new ApplicationTypeId(Guid.Parse("40000000-0000-0000-0000-000000000008")),
            DateTime.UtcNow,
            tenantId: TestTenant.Id);

        var version = definition.StartDraftVersion();
        var first = definition.AddStep("A", "First", durationDays: 5);
        var second = definition.AddStep("B", "Second", durationDays: 3);
        definition.AddStepPredecessor(second.Id, first.Id);
        definition.PublishVersion(version.Id, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        context.ProcessDefinitions.Add(definition);
        await context.SaveChangesAsync();

        return definition;
    }

    private static async Task<ProcessObjectiveId> AnObjectiveAsync(
        RegOSDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var product = GlobalProduct.Register(
            TestTenant.Id, $"PLAN-{suffix}", $"Plan fixture {suffix}",
            ProductType.Drug);

        context.Products.Add(product);

        var objective = ProcessObjective.Create(
            TestTenant.Id, product.Id, UnitedStates,
            "Open an IND in the US", Anchor);

        context.ProcessObjectives.Add(objective);

        await context.SaveChangesAsync();

        return objective.Id;
    }

    private sealed class FixedTenant : ITenantContext
    {
        public TenantId TenantId => TestTenant.Id;

        public TenantId? TenantIdOrNull => TestTenant.Id;
    }
}
