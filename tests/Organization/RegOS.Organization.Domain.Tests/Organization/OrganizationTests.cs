using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Exceptions;

using OrganizationAggregate =
    RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Organization.Domain.Tests.Organization;

public sealed class OrganizationTests
{
    [Fact]
    public void Creates_an_organization_with_the_supplied_details()
    {
        var organization = OrganizationAggregate.Create(
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
            "Acme Pharma Ltd.",
            OrganizationType.Sponsor);

        organization.Status.Should().Be(OrganizationStatus.Active);
    }

    [Fact]
    public void Assigns_an_identity()
    {
        var organization = OrganizationAggregate.Create(
            "Acme Pharma Ltd.",
            OrganizationType.Sponsor);

        organization.Id.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Trims_the_legal_name()
    {
        var organization = OrganizationAggregate.Create(
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
            "Acme Pharma Ltd.",
            (OrganizationType)99);

        act.Should().Throw<DomainException>()
            .WithMessage(OrganizationErrors.TypeInvalid);
    }

    [Fact]
    public void Creates_with_a_caller_supplied_identity()
    {
        // The seeder uses this overload so demo data keeps stable ids.
        var id = OrganizationId.New();

        var organization = OrganizationAggregate.Create(
            id,
            "Acme Pharma Ltd.",
            OrganizationType.ContractResearchOrganization);

        organization.Id.Should().Be(id);
    }
}
