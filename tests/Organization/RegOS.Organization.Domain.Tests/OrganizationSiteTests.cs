using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Organization.Domain.Tests;

public class OrganizationSiteTests
{
    private static readonly DateOnly Opened = new(2014, 5, 1);
    private static readonly CountryId India = new(Guid.NewGuid());

    private static OrganizationSite New(
        PostalAddress? address = null,
        DateOnly? statusDate = null) =>
        OrganizationSite.Create(
            TenantId.New(),
            OrganizationId.New(),
            "Hyderabad Plant",
            OrganizationSiteType.Manufacturing,
            address ?? PostalAddress.Create(India),
            statusDate ?? Opened);

    // --- Creation ------------------------------------------------------------

    [Fact]
    public void ANewSiteIsActiveFromTheDateItOpened()
    {
        var site = New();

        site.Status.Should().Be(OrganizationStatus.Active);
        site.StatusDate.Should().Be(Opened);
    }

    [Fact]
    public void ASiteStartsWithNoIdentifiers()
    {
        New().Identifiers.Should().BeEmpty();
    }

    [Fact]
    public void ANameIsRequired()
    {
        var create = () => OrganizationSite.Create(
            TenantId.New(),
            OrganizationId.New(),
            "   ",
            OrganizationSiteType.Manufacturing,
            PostalAddress.Create(India),
            Opened);

        create.Should().Throw<DomainException>()
            .WithMessage(OrganizationSiteErrors.NameRequired);
    }

    [Fact]
    public void ATypeOutsideTheEnumIsRejected()
    {
        var create = () => OrganizationSite.Create(
            TenantId.New(),
            OrganizationId.New(),
            "Plant",
            (OrganizationSiteType)99,
            PostalAddress.Create(India),
            Opened);

        create.Should().Throw<DomainException>()
            .WithMessage(OrganizationSiteErrors.TypeInvalid);
    }

    [Fact]
    public void AStatusDateIsRequired()
    {
        var create = () => New(statusDate: default(DateOnly));

        create.Should().Throw<DomainException>()
            .WithMessage(OrganizationSiteErrors.StatusDateRequired);
    }

    // --- The address ---------------------------------------------------------

    /// <summary>
    /// The whole reason the value object is weak: an in-licensed asset arrives
    /// as a manufacturer name and a country, and refusing that would lose the
    /// fact entirely (the ADR-035 principle).
    /// </summary>
    [Fact]
    public void ACountryAloneIsEnoughOfAnAddress()
    {
        var address = PostalAddress.Create(India);

        address.CountryId.Should().Be(India);
        address.Line1.Should().BeNull();
        address.City.Should().BeNull();
        address.PostalCode.Should().BeNull();
    }

    [Fact]
    public void AnAddressMustNameACountry()
    {
        var create = () => PostalAddress.Create(default);

        create.Should().Throw<DomainException>()
            .WithMessage(OrganizationSiteErrors.CountryRequired);
    }

    [Fact]
    public void BlankAddressPartsAreStoredAsNull()
    {
        var address = PostalAddress.Create(India, line1: "  ", city: "   ");

        address.Line1.Should().BeNull();
        address.City.Should().BeNull();
    }

    [Fact]
    public void AddressPartsAreTrimmed()
    {
        PostalAddress.Create(India, city: "  Hyderabad  ")
            .City.Should().Be("Hyderabad");
    }

    [Fact]
    public void AnOverlongAddressLineIsRejected()
    {
        var create = () => PostalAddress.Create(
            India, line1: new string('x', 201));

        create.Should().Throw<DomainException>()
            .WithMessage(OrganizationSiteErrors.AddressLineTooLong);
    }

    [Fact]
    public void ASiteCanBeRelocated()
    {
        var site = New();
        var elsewhere = PostalAddress.Create(
            new CountryId(Guid.NewGuid()), city: "Singapore");

        site.Relocate(elsewhere);

        site.Address.City.Should().Be("Singapore");
    }

    // --- Identifiers ---------------------------------------------------------

    /// <summary>
    /// The reason this is a collection rather than a field: a US plant holds an
    /// FEI and a DUNS number today, and they are peers.
    /// </summary>
    [Fact]
    public void ASiteMayHoldIdentifiersFromSeveralSchemes()
    {
        var site = New();
        var fei = IdentifierSchemeId.New();
        var duns = IdentifierSchemeId.New();

        site.AddIdentifier(fei, "3001234567");
        site.AddIdentifier(duns, "150483782");

        site.Identifiers.Should().HaveCount(2);
    }

    /// <summary>
    /// The aggregate invariant the database also enforces with a unique index:
    /// a second FEI would mean one of them is wrong, not that the site has two.
    /// </summary>
    [Fact]
    public void ASiteCannotHoldTwoIdentifiersFromTheSameScheme()
    {
        var site = New();
        var fei = IdentifierSchemeId.New();
        site.AddIdentifier(fei, "3001234567");

        var again = () => site.AddIdentifier(fei, "3009999999");

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(OrganizationSiteErrors.IdentifierSchemeAlreadyRecorded);

        site.Identifiers.Should().ContainSingle();
    }

    [Fact]
    public void AnIdentifierNeedsAScheme()
    {
        var site = New();

        var add = () => site.AddIdentifier(default, "3001234567");

        add.Should().Throw<DomainException>()
            .WithMessage(OrganizationSiteErrors.IdentifierSchemeRequired);
    }

    [Fact]
    public void AnIdentifierNeedsAValue()
    {
        var site = New();

        var add = () => site.AddIdentifier(IdentifierSchemeId.New(), " ");

        add.Should().Throw<DomainException>()
            .WithMessage(OrganizationSiteErrors.IdentifierValueRequired);
    }

    /// <summary>
    /// Identifiers are current facts about the site, not a record of events, so
    /// a mistyped FEI is correctable — unlike a registration's history.
    /// </summary>
    [Fact]
    public void AnIdentifierCanBeRemoved()
    {
        var site = New();
        var identifier = site.AddIdentifier(IdentifierSchemeId.New(), "3001234567");

        site.RemoveIdentifier(identifier.Id);

        site.Identifiers.Should().BeEmpty();
    }

    [Fact]
    public void RemovingAnIdentifierThatIsNotThereIsNotFound()
    {
        var site = New();

        var remove = () => site.RemoveIdentifier(SiteIdentifierId.New());

        remove.Should().Throw<NotFoundException>()
            .WithMessage(OrganizationSiteErrors.IdentifierNotFound);
    }

    [Fact]
    public void AFailedIdentifierLeavesTheSiteUntouched()
    {
        var site = New();

        var add = () => site.AddIdentifier(IdentifierSchemeId.New(), "");
        add.Should().Throw<DomainException>();

        site.Identifiers.Should().BeEmpty();
    }

    // --- Activation, not lifecycle -------------------------------------------

    /// <summary>
    /// Status here is an activation flag, not a business lifecycle: it answers
    /// "do we still use this place?" rather than recording a position an
    /// authority took. So it carries a date and no history — the same treatment
    /// Organization and Product already have.
    /// </summary>
    [Fact]
    public void DeactivatingRecordsTheDateItClosed()
    {
        var site = New();
        var closed = new DateOnly(2025, 3, 31);

        site.Deactivate(closed);

        site.Status.Should().Be(OrganizationStatus.Inactive);
        site.StatusDate.Should().Be(closed);
    }

    [Fact]
    public void ReactivatingRecordsTheDateItReopened()
    {
        var site = New();
        site.Deactivate(new DateOnly(2025, 3, 31));

        site.Activate(new DateOnly(2026, 1, 15));

        site.Status.Should().Be(OrganizationStatus.Active);
        site.StatusDate.Should().Be(new DateOnly(2026, 1, 15));
    }

    [Fact]
    public void DeactivatingTwiceIsRefused()
    {
        var site = New();
        site.Deactivate(new DateOnly(2025, 3, 31));

        var again = () => site.Deactivate(new DateOnly(2025, 4, 1));

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(OrganizationSiteErrors.AlreadyInactive);
    }

    [Fact]
    public void ActivatingAnActiveSiteIsRefused()
    {
        var activate = () => New().Activate(new DateOnly(2026, 1, 1));

        activate.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(OrganizationSiteErrors.AlreadyActive);
    }

    [Fact]
    public void AStatusChangeNeedsItsDate()
    {
        var site = New();

        var deactivate = () => site.Deactivate(default);

        deactivate.Should().Throw<DomainException>()
            .WithMessage(OrganizationSiteErrors.StatusDateRequired);
    }
}
