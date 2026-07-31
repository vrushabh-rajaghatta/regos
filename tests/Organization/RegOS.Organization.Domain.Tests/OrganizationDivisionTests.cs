using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationDivision;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

using DivisionAggregate = RegOS.Organization.Domain.Aggregates.OrganizationDivision.OrganizationDivision;
using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Organization.Domain.Tests;

public class OrganizationDivisionTests
{
    private static readonly DateOnly Established = new(2018, 4, 1);

    private static DivisionAggregate New(string name = "Regulatory Affairs") =>
        DivisionAggregate.Create(
            TenantId.New(), OrganizationId.New(), name, Established, "RA");

    [Fact]
    public void ANewDivisionIsActiveFromTheDateItWasEstablished()
    {
        var division = New();

        division.Status.Should().Be(OrganizationStatus.Active);
        division.StatusDate.Should().Be(Established);
        division.Acronym.Should().Be("RA");
    }

    [Fact]
    public void ANameIsRequired()
    {
        var create = () => New(name: "  ");

        create.Should().Throw<DomainException>()
            .WithMessage(OrganizationDivisionErrors.NameRequired);
    }

    [Fact]
    public void DeactivatingRecordsTheDateItWasDissolved()
    {
        var division = New();

        division.Deactivate(new DateOnly(2025, 12, 31));

        division.Status.Should().Be(OrganizationStatus.Inactive);
        division.StatusDate.Should().Be(new DateOnly(2025, 12, 31));
    }

    [Fact]
    public void DeactivatingTwiceIsRefused()
    {
        var division = New();
        division.Deactivate(new DateOnly(2025, 12, 31));

        var again = () => division.Deactivate(new DateOnly(2026, 1, 1));

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(OrganizationDivisionErrors.AlreadyInactive);
    }
}

/// <summary>
/// The identity attributes STORY-003 adds to Organization itself. Deliberately
/// only three — the aggregate answers "who are we?", not "everything we have
/// ever known about this company".
/// </summary>
public class OrganizationIdentityTests
{
    private static OrganizationAggregate New() =>
        OrganizationAggregate.Create(
            TenantId.New(), "Demo MAH Ltd.", OrganizationType.Manufacturer);

    [Fact]
    public void AnOrganizationRecordsWhenItsStatusTookEffect()
    {
        var organization = New();
        var stopped = new DateOnly(2026, 6, 30);

        organization.Deactivate(stopped);

        organization.StatusDate.Should().Be(stopped);
    }

    /// <summary>
    /// The date is optional here and required on a site or a contact: this
    /// factory predates the field and every existing caller is a UI action
    /// happening now. Omitting it records today.
    /// </summary>
    [Fact]
    public void OmittingTheDateRecordsToday()
    {
        New().StatusDate.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    [Fact]
    public void AnOrganizationCarriesItsShortFormAndLocalScriptName()
    {
        var organization = New();

        organization.DescribeAs("DML", "デモ製薬株式会社");

        organization.Acronym.Should().Be("DML");
        organization.NameNativeLanguage.Should().Be("デモ製薬株式会社");
    }

    /// <summary>
    /// Companies routinely hold several at once — DUNS, VAT, company
    /// registration — and they are peers.
    /// </summary>
    [Fact]
    public void AnOrganizationMayHoldIdentifiersFromSeveralSchemes()
    {
        var organization = New();

        organization.AddIdentifier(IdentifierSchemeId.New(), "150483782");
        organization.AddIdentifier(IdentifierSchemeId.New(), "GB123456789");

        organization.Identifiers.Should().HaveCount(2);
    }

    [Fact]
    public void AnOrganizationCannotHoldTwoIdentifiersFromTheSameScheme()
    {
        var organization = New();
        var duns = IdentifierSchemeId.New();
        organization.AddIdentifier(duns, "150483782");

        var again = () => organization.AddIdentifier(duns, "999999999");

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(OrganizationErrors.IdentifierSchemeAlreadyRecorded);
    }

    [Fact]
    public void AnIdentifierCanBeCorrected()
    {
        var organization = New();
        var identifier = organization.AddIdentifier(
            IdentifierSchemeId.New(), "150483782");

        organization.RemoveIdentifier(identifier.Id);

        organization.Identifiers.Should().BeEmpty();
    }
}
