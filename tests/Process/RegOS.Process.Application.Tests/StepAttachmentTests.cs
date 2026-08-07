using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Application.Queries.GetProcessPlan;
using RegOS.Process.Application.Tests.Fixtures;
using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Registration.Domain.Aggregates.Registration;

// The alias slice-conventions.md warns about: Aggregates/Registration/ is a
// SINGULAR folder, so its namespace equals the type name. New contexts use the
// plural form for exactly this reason — Process does.
using RegistrationAggregate =
    RegOS.Registration.Domain.Aggregates.Registration.Registration;

namespace RegOS.Process.Application.Tests;

/// <summary>
/// <b>The claim S006 exists to make true:</b> an existing regulatory aggregate
/// can participate in a plan without changing its lifecycle, ownership or
/// business meaning.
/// </summary>
/// <remarks>
/// Every test here is really about ADR-065 <b>I9</b> — an attachment is
/// descriptive, not constitutive. The two that matter most assert what does
/// <em>not</em> happen: a submission is unchanged by being attached, and a step
/// is unchanged by what is attached to it.
/// </remarks>
[Collection(ProcessDatabase.Collection)]
public sealed class StepAttachmentTests
{
    private static readonly CountryId UnitedStates =
        new(Guid.Parse("10000000-0000-0000-0000-000000000001"));

    private static readonly AuthorityId Fda =
        new(Guid.Parse("20000000-0000-0000-0000-000000000001"));

    private static readonly DateOnly Anchor = new(2026, 9, 1);

    private readonly ProcessDatabase _database;

    public StepAttachmentTests(ProcessDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// <b>I9, stated as a test.</b> A registration attached to a step is the same
    /// registration it was — same status, same dates, same number.
    /// </summary>
    [Fact]
    public async Task Attaching_changes_nothing_about_the_record()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var (plan, registration) = await APlanAndRegistration(context);
        var step = plan.Steps.OrderBy(x => x.Code, StringComparer.Ordinal).First();

        var before = await Snapshot(registration.Id);

        await Attach(registration.Id, step.Id);

        var after = await Snapshot(registration.Id);

        after.Should().BeEquivalentTo(before,
            because: "an attachment records that a record contributes to planned "
                + "work; it does not change what the record is");
    }

    /// <summary>
    /// And the reverse: the plan's own schedule is untouched by anything being
    /// attached to it. Process gains a reader, not an owner.
    /// </summary>
    [Fact]
    public async Task Attaching_changes_nothing_about_the_plan()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var (plan, registration) = await APlanAndRegistration(context);
        var step = plan.Steps.OrderBy(x => x.Code, StringComparer.Ordinal).First();

        var before = await Read(context, plan.Id);

        await Attach(registration.Id, step.Id);

        await using var reread = _database.NewContext(TestTenant.Context);
        var after = await Read(reread, plan.Id);

        after.Steps.Select(x => new { x.Code, x.PlannedStartOn, x.PlannedEndOn, x.Status })
            .Should().BeEquivalentTo(
                before.Steps.Select(
                    x => new { x.Code, x.PlannedStartOn, x.PlannedEndOn, x.Status }));
    }

    /// <summary>The link is discoverable from both ends — which is its whole job.</summary>
    [Fact]
    public async Task The_step_shows_what_was_attached_to_it()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var (plan, registration) = await APlanAndRegistration(context);
        var step = plan.Steps.OrderBy(x => x.Code, StringComparer.Ordinal).First();

        await Attach(registration.Id, step.Id);

        await using var reread = _database.NewContext(TestTenant.Context);
        var details = await Read(reread, plan.Id);

        var attached = details.Steps
            .Single(x => x.Id == step.Id.Value)
            .Attached.Should().ContainSingle().Subject;

        attached.Kind.Should().Be("Registration");
        attached.Id.Should().Be(registration.Id.Value);
    }

    /// <summary>
    /// <b>An empty list means nothing</b> (I9). Every other step in the plan has
    /// no attachments and is neither incomplete nor invalid.
    /// </summary>
    [Fact]
    public async Task An_unattached_step_reports_an_empty_list_and_nothing_more()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var (plan, _) = await APlanAndRegistration(context);

        var details = await Read(context, plan.Id);

        details.Steps.Should().OnlyContain(x => x.Attached.Count == 0);
        details.Steps.Should().OnlyContain(x => x.Status == "NotStarted");
    }

    /// <summary>Clearing is always permitted — it changes discoverability only.</summary>
    [Fact]
    public async Task Detaching_leaves_both_ends_as_they_were()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var (plan, registration) = await APlanAndRegistration(context);
        var step = plan.Steps.OrderBy(x => x.Code, StringComparer.Ordinal).First();

        var before = await Snapshot(registration.Id);

        await Attach(registration.Id, step.Id);
        await Attach(registration.Id, null);

        (await Snapshot(registration.Id)).Should().BeEquivalentTo(before);

        await using var reread = _database.NewContext(TestTenant.Context);
        (await Read(reread, plan.Id)).Steps
            .Should().OnlyContain(x => x.Attached.Count == 0);
    }

    // --- fixtures ------------------------------------------------------------

    private static Task<ProcessPlanDetails> Read(
        RegOSDbContext context, ProcessPlanId id)
        => new GetProcessPlanHandler(context)
            .HandleAsync(new GetProcessPlanQuery(id.Value));

    /// <summary>
    /// Everything about the registration except the attachment — so the
    /// comparison proves the attachment changed nothing else.
    /// </summary>
    private async Task<object> Snapshot(RegistrationId id)
    {
        await using var context = _database.NewContext(TestTenant.Context);

        return await context.Registrations
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => (object)new
            {
                x.CurrentStatus,
                x.ApprovedOn,
                x.ExpiresOn,
                x.RegistrationNumber,
                x.MedicinalProductId,
                x.AuthorityId
            })
            .FirstAsync();
    }

    private async Task Attach(RegistrationId id, ProcessStepId? stepId)
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var registration = await context.Registrations.FirstAsync(x => x.Id == id);

        registration.AttachToStep(stepId);

        await context.SaveChangesAsync();
    }

    private static async Task<(ProcessPlan Plan, RegistrationAggregate Registration)>
        APlanAndRegistration(RegOSDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var product = GlobalProduct.Register(
            TestTenant.Id, $"ATT-{suffix}", $"Attach fixture {suffix}",
            ProductType.Drug);

        context.Products.Add(product);

        var market = MedicinalProduct.Create(
            TestTenant.Id, product.Id, UnitedStates, new DateOnly(2026, 1, 1));

        context.MedicinalProducts.Add(market);

        var registration = RegistrationAggregate.Create(
            TestTenant.Id,
            market.Id,
            Fda,
            // Demo Manufacturer Ltd. — the holder the seed guarantees.
            new OrganizationId(Guid.Parse("30000000-0000-0000-0000-000000000001")),
            new DateOnly(2026, 1, 1));

        context.Registrations.Add(registration);

        var objective = ProcessObjective.Create(
            TestTenant.Id, product.Id, UnitedStates, "Open an IND", Anchor);

        context.ProcessObjectives.Add(objective);

        var definition = ProcessDefinition.Create(
            $"ATT-{suffix}", $"Attach playbook {suffix}",
            UnitedStates, Fda,
            new ApplicationTypeId(Guid.Parse("40000000-0000-0000-0000-000000000008")),
            DateTime.UtcNow, tenantId: TestTenant.Id);

        var version = definition.StartDraftVersion();
        definition.AddStep("A", "First", durationDays: 5);
        definition.AddStep("B", "Second", durationDays: 3);
        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        context.ProcessDefinitions.Add(definition);

        var plan = ProcessPlan.InstantiateFrom(
            TestTenant.Id, objective.Id, version, Anchor, "Filing plan", Anchor);

        context.ProcessPlans.Add(plan);

        await context.SaveChangesAsync();

        return (plan, registration);
    }
}
