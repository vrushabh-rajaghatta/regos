using RegOS.SharedKernel.Primitives;
using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Exceptions;

using OrganizationAggregate =
    RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Organization.Domain.Tests.Organization;

public sealed class OrganizationTests
{
    private static OrganizationAggregate Active() =>
        OrganizationAggregate.Create(
            TenantId.New(),
            "Acme Pharma Ltd.",
            OrganizationType.Manufacturer);

    [Fact]
    public void Creates_an_organization_with_the_supplied_details()
    {
        var organization = OrganizationAggregate.Create(
            TenantId.New(),
            "Acme Pharma Ltd.",
            OrganizationType.Manufacturer);

        organization.LegalName.Should().Be("Acme Pharma Ltd.");
        organization.Type.Should().Be(OrganizationType.Manufacturer);
    }

    [Fact]
    public void Starts_active()
    {
        // Status is the domain's to set, which is why it is not a create field.
        var organization = OrganizationAggregate.Create(
            TenantId.New(),
            "Acme Pharma Ltd.",
            OrganizationType.Sponsor);

        organization.Status.Should().Be(OrganizationStatus.Active);
    }

    [Fact]
    public void Assigns_an_identity()
    {
        var organization = OrganizationAggregate.Create(
            TenantId.New(),
            "Acme Pharma Ltd.",
            OrganizationType.Sponsor);

        organization.Id.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Trims_the_legal_name()
    {
        var organization = OrganizationAggregate.Create(
            TenantId.New(),
            "   Acme Pharma Ltd.   ",
            OrganizationType.Manufacturer);

        organization.LegalName.Should().Be("Acme Pharma Ltd.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_a_missing_legal_name(string? legalName)
    {
        var act = () => OrganizationAggregate.Create(
            TenantId.New(),
            legalName!,
            OrganizationType.Manufacturer);

        act.Should().Throw<DomainException>()
            .WithMessage(OrganizationErrors.LegalNameRequired);
    }

    [Fact]
    public void Rejects_a_type_outside_the_defined_values()
    {
        // Model binding turns {"type": 99} into an OrganizationType without
        // complaint, so the aggregate is the only place this can be stopped.
        var act = () => OrganizationAggregate.Create(
            TenantId.New(),
            "Acme Pharma Ltd.",
            (OrganizationType)99);

        act.Should().Throw<DomainException>()
            .WithMessage(OrganizationErrors.TypeInvalid);
    }

    [Fact]
    public void Deactivates_an_active_organization()
    {
        var organization = OrganizationAggregate.Create(
            TenantId.New(),
            "Acme Pharma Ltd.",
            OrganizationType.Manufacturer);

        organization.Deactivate();

        organization.Status.Should().Be(OrganizationStatus.Inactive);
    }

    [Fact]
    public void Rejects_deactivating_an_already_inactive_organization()
    {
        // A silent no-op would tell a caller with a stale view that the
        // operation succeeded. Valid request, forbidden state: 409.
        var organization = OrganizationAggregate.Create(
            TenantId.New(),
            "Acme Pharma Ltd.",
            OrganizationType.Manufacturer);

        organization.Deactivate();

        var act = () => organization.Deactivate();

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(OrganizationErrors.AlreadyInactive);
    }

    [Fact]
    public void Activates_an_inactive_organization()
    {
        var organization = Active();
        organization.Deactivate();

        organization.Activate();

        organization.Status.Should().Be(OrganizationStatus.Active);
    }

    [Fact]
    public void Rejects_activating_an_already_active_organization()
    {
        // The mirror of Deactivate, rejected the same way: there is no
        // transition to make, so this is a conflict rather than a no-op.
        var organization = Active();

        var act = () => organization.Activate();

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(OrganizationErrors.AlreadyActive);
    }

    [Fact]
    public void Round_trips_through_the_full_lifecycle()
    {
        var organization = Active();

        organization.Deactivate();
        organization.Activate();
        organization.Deactivate();

        organization.Status.Should().Be(OrganizationStatus.Inactive);
    }

    [Fact]
    public void Keeps_its_details_when_deactivated()
    {
        // Deactivation retires the organization; it does not erase it.
        var organization = OrganizationAggregate.Create(
            TenantId.New(),
            "Acme Pharma Ltd.",
            OrganizationType.Sponsor);

        organization.Deactivate();

        organization.LegalName.Should().Be("Acme Pharma Ltd.");
        organization.Type.Should().Be(OrganizationType.Sponsor);
    }

    [Fact]
    public void Renames_the_organization()
    {
        var organization = Active();

        organization.Rename("  Acme Pharmaceuticals Ltd.  ");

        organization.LegalName.Should().Be("Acme Pharmaceuticals Ltd.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_renaming_to_a_missing_legal_name(string? legalName)
    {
        var organization = Active();

        var act = () => organization.Rename(legalName);

        act.Should().Throw<DomainException>()
            .WithMessage(OrganizationErrors.LegalNameRequired);
    }

    [Fact]
    public void Reclassifies_the_organization()
    {
        var organization = Active();

        organization.Reclassify(OrganizationType.ContractResearchOrganization);

        organization.Type.Should()
            .Be(OrganizationType.ContractResearchOrganization);
    }

    [Fact]
    public void Rejects_reclassifying_to_an_undefined_type()
    {
        var organization = Active();

        var act = () => organization.Reclassify((OrganizationType)99);

        act.Should().Throw<DomainException>()
            .WithMessage(OrganizationErrors.TypeInvalid);
    }

    [Fact]
    public void Allows_editing_an_inactive_organization()
    {
        // Deliberate: deactivation says "do not start new work with this", not
        // "freeze the record". A misspelled legal name is worth correcting
        // whether or not the organization is still trading. Product takes the
        // same position — an archived product can still be renamed.
        var organization = Active();
        organization.Deactivate();

        organization.Rename("Acme Pharmaceuticals Ltd.");
        organization.Reclassify(OrganizationType.Sponsor);

        organization.LegalName.Should().Be("Acme Pharmaceuticals Ltd.");
        organization.Type.Should().Be(OrganizationType.Sponsor);
        organization.Status.Should().Be(OrganizationStatus.Inactive);
    }

    [Fact]
    public void Editing_does_not_change_status()
    {
        // Status belongs to Activate and Deactivate. An edit is a correction to
        // a record, never a lifecycle transition.
        var organization = Active();

        organization.Rename("Acme Pharmaceuticals Ltd.");
        organization.Reclassify(OrganizationType.Sponsor);

        organization.Status.Should().Be(OrganizationStatus.Active);
    }

    [Fact]
    public void Renaming_to_the_same_value_is_a_no_op()
    {
        // No version to increment and nothing to reject: submitting unchanged
        // values simply leaves the aggregate as it was.
        var organization = Active();

        organization.Rename("Acme Pharma Ltd.");

        organization.LegalName.Should().Be("Acme Pharma Ltd.");
    }

    [Fact]
    public void Creates_with_a_caller_supplied_identity()
    {
        // The seeder uses this overload so demo data keeps stable ids.
        var id = OrganizationId.New();

        var organization = OrganizationAggregate.Create(
            id,
            TenantId.New(),
            "Acme Pharma Ltd.",
            OrganizationType.ContractResearchOrganization);

        organization.Id.Should().Be(id);
    }

    [Fact]
    public void Requires_a_tenant()
    {
        // The registry is tenant-owned (ADR-032): an organization outside any
        // tenant's registry would be visible to nobody and mutable by nobody.
        var act = () => OrganizationAggregate.Create(
            null!,
            "Acme Pharma Ltd.",
            OrganizationType.Manufacturer);

        act.Should().Throw<DomainException>()
            .WithMessage(OrganizationAggregate.TenantRequired);
    }
}
