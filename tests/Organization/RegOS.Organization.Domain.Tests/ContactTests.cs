using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

using ContactAggregate = RegOS.Organization.Domain.Aggregates.Contact.Contact;

namespace RegOS.Organization.Domain.Tests;

public class ContactTests
{
    private static readonly DateOnly Joined = new(2021, 9, 1);

    private static ContactAggregate New(
        string first = "Priya",
        string last = "Raman",
        DateOnly? statusDate = null) =>
        ContactAggregate.Create(
            TenantId.New(),
            OrganizationId.New(),
            first,
            last,
            statusDate ?? Joined);

    // --- Creation ------------------------------------------------------------

    [Fact]
    public void ANewContactIsActiveFromTheDateTheyJoined()
    {
        var contact = New();

        contact.Status.Should().Be(OrganizationStatus.Active);
        contact.StatusDate.Should().Be(Joined);
    }

    /// <summary>
    /// RIM makes the site required. Relaxed deliberately: an authority reviewer
    /// or a head-office regulatory lead has no site, and refusing them would
    /// lose the person entirely.
    /// </summary>
    [Fact]
    public void AContactNeedsNoSite()
    {
        New().OrganizationSiteId.Should().BeNull();
    }

    /// <summary>
    /// Unlike a site's country, which the directory filters by. Nothing here
    /// reasons about a contact's country.
    /// </summary>
    [Fact]
    public void AContactNeedsNoCountry()
    {
        New().CountryId.Should().BeNull();
    }

    [Fact]
    public void BothNamesAreRequired()
    {
        var noFirst = () => New(first: "  ");
        noFirst.Should().Throw<DomainException>()
            .WithMessage(ContactErrors.FirstNameRequired);

        var noLast = () => New(last: "");
        noLast.Should().Throw<DomainException>()
            .WithMessage(ContactErrors.LastNameRequired);
    }

    [Fact]
    public void NamesAreTrimmed()
    {
        var contact = New(first: "  Priya ", last: " Raman  ");

        contact.FirstName.Should().Be("Priya");
        contact.LastName.Should().Be("Raman");
    }

    [Fact]
    public void AStatusDateIsRequired()
    {
        var create = () => New(statusDate: default(DateOnly));

        create.Should().Throw<DomainException>()
            .WithMessage(ContactErrors.StatusDateRequired);
    }

    // --- Roles ---------------------------------------------------------------

    /// <summary>
    /// Several roles on one person is ordinary — a site's regulatory contact is
    /// often also its QP.
    /// </summary>
    [Fact]
    public void AContactMayHoldSeveralRoles()
    {
        var contact = New();

        contact.AddRole(ContactRoleId.New());
        contact.AddRole(ContactRoleId.New());

        contact.Roles.Should().HaveCount(2);
    }

    [Fact]
    public void AContactCannotHoldTheSameRoleTwice()
    {
        var contact = New();
        var qp = ContactRoleId.New();
        contact.AddRole(qp);

        var again = () => contact.AddRole(qp);

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ContactErrors.RoleAlreadyHeld);

        contact.Roles.Should().ContainSingle();
    }

    [Fact]
    public void ARoleCanBeGivenUp()
    {
        var contact = New();
        var qp = ContactRoleId.New();
        contact.AddRole(qp);

        contact.RemoveRole(qp);

        contact.Roles.Should().BeEmpty();
    }

    [Fact]
    public void RemovingARoleTheyDoNotHoldIsNotFound()
    {
        var remove = () => New().RemoveRole(ContactRoleId.New());

        remove.Should().Throw<NotFoundException>()
            .WithMessage(ContactErrors.RoleNotHeld);
    }

    // --- Emails and phones ---------------------------------------------------

    [Fact]
    public void AContactMayHaveSeveralEmailsAndPhones()
    {
        var contact = New();

        contact.AddEmail("priya.raman@example.com");
        contact.AddEmail("qp@example.com");
        contact.AddPhone("+91 40 1234 5678");
        contact.AddPhone("+91 98765 43210");

        contact.Emails.Should().HaveCount(2);
        contact.Phones.Should().HaveCount(2);
    }

    /// <summary>
    /// Recorded twice would say the same thing twice. Case-insensitive, because
    /// an address differing only in case is the same address.
    /// </summary>
    [Fact]
    public void TheSameEmailIsNotRecordedTwice()
    {
        var contact = New();
        contact.AddEmail("priya.raman@example.com");

        var again = () => contact.AddEmail("Priya.Raman@Example.com");

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ContactErrors.EmailAlreadyRecorded);
    }

    [Fact]
    public void TheSamePhoneIsNotRecordedTwice()
    {
        var contact = New();
        contact.AddPhone("+91 40 1234 5678");

        var again = () => contact.AddPhone("+91 40 1234 5678");

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ContactErrors.PhoneAlreadyRecorded);
    }

    /// <summary>
    /// The one structural check worth making — the mistake it catches is a
    /// mis-pasted field, not an unusual address.
    /// </summary>
    [Fact]
    public void SomethingWithNoAtSignIsNotAnEmailAddress()
    {
        var add = () => New().AddEmail("priya.raman.example.com");

        add.Should().Throw<DomainException>()
            .WithMessage(ContactErrors.EmailNotAnAddress);
    }

    /// <summary>
    /// International formats vary too much for RegOS to have an opinion, and
    /// normalising would lose the extension a user typed.
    /// </summary>
    [Fact]
    public void APhoneNumberIsKeptExactlyAsWritten()
    {
        var contact = New();

        contact.AddPhone("  +91 (40) 1234-5678 ext. 22  ");

        contact.Phones.Single().Number
            .Should().Be("+91 (40) 1234-5678 ext. 22");
    }

    [Fact]
    public void ABlankEmailIsRejected()
    {
        var add = () => New().AddEmail("   ");

        add.Should().Throw<DomainException>()
            .WithMessage(ContactErrors.EmailRequired);
    }

    [Fact]
    public void AFailedEmailLeavesTheContactUntouched()
    {
        var contact = New();

        var add = () => contact.AddEmail("not-an-address");
        add.Should().Throw<DomainException>();

        contact.Emails.Should().BeEmpty();
    }

    // --- Activation, not lifecycle -------------------------------------------

    /// <summary>
    /// Like a site and unlike a registration: "do not use this contact" is
    /// configuration, not a regulatory event, so there is a date and no history.
    /// </summary>
    [Fact]
    public void DeactivatingRecordsTheDateTheyLeft()
    {
        var contact = New();
        var left = new DateOnly(2026, 2, 28);

        contact.Deactivate(left);

        contact.Status.Should().Be(OrganizationStatus.Inactive);
        contact.StatusDate.Should().Be(left);
    }

    [Fact]
    public void DeactivatingTwiceIsRefused()
    {
        var contact = New();
        contact.Deactivate(new DateOnly(2026, 2, 28));

        var again = () => contact.Deactivate(new DateOnly(2026, 3, 1));

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ContactErrors.AlreadyInactive);
    }

    [Fact]
    public void AReturningContactIsReactivated()
    {
        var contact = New();
        contact.Deactivate(new DateOnly(2026, 2, 28));

        contact.Activate(new DateOnly(2026, 6, 1));

        contact.Status.Should().Be(OrganizationStatus.Active);
        contact.StatusDate.Should().Be(new DateOnly(2026, 6, 1));
    }

    [Fact]
    public void AContactCanBeRenamed()
    {
        var contact = New();

        contact.Rename("Priya", "Raman-Iyer");

        contact.LastName.Should().Be("Raman-Iyer");
    }
}
