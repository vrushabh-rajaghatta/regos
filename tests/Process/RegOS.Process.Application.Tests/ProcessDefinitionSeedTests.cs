using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using RegOS.Process.Application.Queries.GetProcessDefinition;
using RegOS.Process.Application.Queries.ListProcessDefinitions;
using RegOS.Process.Application.Tests.Fixtures;
using RegOS.Process.Domain.Aggregates.ProcessDefinitions;

namespace RegOS.Process.Application.Tests;

/// <summary>
/// The seeded US·FDA·IND playbook, read back through the queries a screen uses.
/// </summary>
/// <remarks>
/// <b>This is the story's end-to-end proof.</b> The database is created from the
/// current migration chain and seeded by the real <c>IDataInitializer</c> chain
/// (ADR-064), so a green run here says the schema, the EF configuration, the
/// aggregate's publish rules and the seed all agree — none of which is provable
/// from a domain test.
/// </remarks>
[Collection(ProcessDatabase.Collection)]
public sealed class ProcessDefinitionSeedTests
{
    private readonly ProcessDatabase _database;

    public ProcessDefinitionSeedTests(ProcessDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task The_seeded_playbook_is_visible_to_a_tenant()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var playbooks = await new ListProcessDefinitionsHandler(context)
            .HandleAsync(new ListProcessDefinitionsQuery());

        var ind = playbooks.Should().ContainSingle(
            x => x.Code == "US-FDA-IND-INITIAL").Subject;

        ind.Name.Should().Be("US FDA IND — initial filing");
        ind.CountryCode.Should().Be("US");
        ind.AuthorityName.Should().Contain("Food and Drug");
        ind.Status.Should().Be(nameof(ProcessDefinitionStatus.Active));
    }

    /// <summary>
    /// <b>The platform's playbook, not the tenant's.</b> The shared-plus-extensible
    /// filter is what makes it visible without the tenant owning it, and
    /// <c>IsShared</c> is what a steward screen reads to know it may not be
    /// edited (EPIC-012).
    /// </summary>
    [Fact]
    public async Task The_seeded_playbook_is_the_platforms_own()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var playbooks = await new ListProcessDefinitionsHandler(context)
            .HandleAsync(new ListProcessDefinitionsQuery());

        playbooks.Single(x => x.Code == "US-FDA-IND-INITIAL")
            .IsShared.Should().BeTrue();
    }

    /// <summary>
    /// The whole point of I4: the seed publishes v1, so it is frozen, and a plan
    /// created tomorrow pins <em>this</em> version.
    /// </summary>
    [Fact]
    public async Task The_seeded_version_is_published_and_carries_twelve_steps()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var details = await Details(context);

        details.SelectedVersionNumber.Should().Be(1);
        details.Versions.Should().ContainSingle()
            .Which.Status.Should().Be(nameof(ProcessDefinitionVersionStatus.Published));
        details.Steps.Should().HaveCount(12);
    }

    /// <summary>
    /// The predecessor graph survives the round trip through Postgres — the one
    /// thing a domain test cannot check, because it never serialises anything.
    /// </summary>
    [Fact]
    public async Task The_step_graph_round_trips()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var details = await Details(context);

        var compile = details.Steps.Single(x => x.Code == "COMPILE");

        compile.Predecessors.Should().BeEquivalentTo(
            ["CMC", "FORMS", "IB"],
            because: "three strands converge on compilation, and the read "
                + "resolves each predecessor id back to the code a reader knows");

        details.Steps.Single(x => x.Code == "PRE-IND-REQ")
            .Predecessors.Should().BeEmpty(
                "the first step waits for the plan's anchor, not for a step");
    }

    /// <summary>
    /// Offsets and durations are what make S003's derivation possible, so they
    /// have to survive persistence intact.
    /// </summary>
    [Fact]
    public async Task Offsets_and_durations_round_trip()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var details = await Details(context);

        var package = details.Steps.Single(x => x.Code == "PRE-IND-PKG");

        package.OffsetDays.Should().Be(30);
        package.DurationDays.Should().Be(30);

        details.Steps.Single(x => x.Code == "SAFETY-30")
            .DurationDays.Should().Be(30);
    }

    /// <summary>
    /// Reading it twice returns the same order. The plan board is the most
    /// ordering-dense read RegOS has, and <c>Order</c> is deliberately not
    /// unique — the code is what makes the pair total.
    /// </summary>
    [Fact]
    public async Task The_steps_come_back_in_the_same_order_every_time()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var first = await Details(context);
        var second = await Details(context);

        second.Steps.Select(x => x.Code).Should().Equal(
            first.Steps.Select(x => x.Code));

        first.Steps.Select(x => x.Code).Should().StartWith(
            new[] { "PRE-IND-REQ", "PRE-IND-PKG", "PRE-IND-MTG" });
    }

    /// <summary>
    /// Only one version exists, so nothing supersedes it and there is no
    /// <c>EffectiveTo</c> to derive. The column does not exist either — that is
    /// the point.
    /// </summary>
    [Fact]
    public async Task A_sole_version_has_no_derived_end_date()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var details = await Details(context);

        var version = details.Versions.Single();

        version.EffectiveFrom.Should().Be(new DateOnly(2026, 8, 6));
        version.EffectiveTo.Should().BeNull();
        version.StepCount.Should().Be(12);
    }

    /// <summary>
    /// Running the initializer again must not produce a second playbook — the
    /// property every boot of the API depends on.
    /// </summary>
    [Fact]
    public async Task The_seed_is_idempotent()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var before = await context.ProcessDefinitions
            .IgnoreQueryFilters()
            .CountAsync();

        await new Persistence.Initialization.Process.ProcessDefinitionDataInitializer(
            _database.NewContext()).InitializeAsync();

        var after = await context.ProcessDefinitions
            .IgnoreQueryFilters()
            .CountAsync();

        after.Should().Be(before);
    }

    private static async Task<ProcessDefinitionDetails> Details(
        Persistence.RegOSDbContext context)
    {
        var playbooks = await new ListProcessDefinitionsHandler(context)
            .HandleAsync(new ListProcessDefinitionsQuery());

        var id = playbooks.Single(x => x.Code == "US-FDA-IND-INITIAL").Id;

        return await new GetProcessDefinitionHandler(context)
            .HandleAsync(new GetProcessDefinitionQuery(id));
    }
}
