using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Organization.Domain.Aggregates.Contact;

/// <summary>
/// A named person in regulatory work — the QP for a product, the regulatory
/// contact at a partner, a reviewer at an authority.
/// </summary>
/// <remarks>
/// An aggregate root for the same reasons as <see cref="OrganizationSite"/>:
/// other aggregates will reference a contact <b>by id</b> (a licence's
/// responsible contacts, an application's QP), and the work asks <em>"who is the
/// QP for this application?"</em> across the whole registry rather than within
/// one company.
/// <para>
/// <b>A Contact is not a User.</b> A user logs in; a contact is a person named
/// in a regulatory record, very often at another company or at an authority.
/// Conflating them would drag Platform identity into the regulatory domain and
/// give one of them the wrong lifecycle. Recorded here so nobody unifies them
/// later without a conversation.
/// </para>
/// <para>
/// Status is an activation flag, not a business lifecycle — <em>do not use this
/// contact</em> is configuration, not a regulatory event — so like a site it
/// carries a <see cref="StatusDate"/> and no history.
/// </para>
/// </remarks>
public sealed class Contact : AggregateRoot<ContactId>
{
    public const int NameMaxLength = 150;
    public const int EmailMaxLength = 320;
    public const int PhoneMaxLength = 50;

    private readonly List<ContactRoleAssignment> _roles = [];
    private readonly List<ContactEmail> _emails = [];
    private readonly List<ContactPhone> _phones = [];

    private Contact()
    {
    }

    /// <summary>
    /// Its own tenant and its own fail-closed filter: a contact is a root,
    /// reachable through the directory rather than only through a filtered
    /// parent, and a named person at a partner is exactly the competitively
    /// sensitive data ADR-032 was written for.
    /// </summary>
    public TenantId TenantId { get; private set; } = default!;

    public OrganizationId OrganizationId { get; private set; } = default!;

    /// <summary>
    /// The site this person works at, when it is known.
    /// </summary>
    /// <remarks>
    /// RIM makes this required. Relaxed deliberately: real contacts routinely
    /// arrive with a company and no site — an authority reviewer, a partner's
    /// head-office regulatory lead — and refusing them would lose the person
    /// entirely.
    /// </remarks>
    public OrganizationSiteId? OrganizationSiteId { get; private set; }

    public string FirstName { get; private set; } = default!;

    public string LastName { get; private set; } = default!;

    public string? Title { get; private set; }

    public string? Department { get; private set; }

    /// <summary>
    /// Optional, unlike a site's country. A site's country is required because
    /// the site directory filters by it; the contact directory filters by role,
    /// so nothing here reasons about the country.
    /// </summary>
    public CountryId? CountryId { get; private set; }

    public OrganizationStatus Status { get; private set; }

    public DateOnly StatusDate { get; private set; }

    public IReadOnlyCollection<ContactRoleAssignment> Roles
        => _roles.AsReadOnly();

    public IReadOnlyCollection<ContactEmail> Emails => _emails.AsReadOnly();

    public IReadOnlyCollection<ContactPhone> Phones => _phones.AsReadOnly();

    public static Contact Create(
        TenantId tenantId,
        OrganizationId organizationId,
        string firstName,
        string lastName,
        DateOnly statusDate,
        OrganizationSiteId? organizationSiteId = null,
        string? title = null,
        string? department = null,
        CountryId? countryId = null)
    {
        if (tenantId is null)
            throw new DomainException(ContactErrors.TenantRequired);

        if (organizationId is null)
            throw new DomainException(ContactErrors.OrganizationRequired);

        if (statusDate == default)
            throw new DomainException(ContactErrors.StatusDateRequired);

        return new Contact
        {
            Id = ContactId.New(),
            TenantId = tenantId,
            OrganizationId = organizationId,
            OrganizationSiteId = organizationSiteId,
            FirstName = Name(firstName, ContactErrors.FirstNameRequired),
            LastName = Name(lastName, ContactErrors.LastNameRequired),
            Title = Trimmed(title),
            Department = Trimmed(department),
            CountryId = countryId,
            Status = OrganizationStatus.Active,
            StatusDate = statusDate,
        };
    }

    /// <summary>
    /// Gives this person a role. One assignment per role — a second would mean
    /// the same thing twice, not that they hold it doubly.
    /// </summary>
    /// <remarks>
    /// No notion of a <em>primary</em> role, deliberately. Several roles on one
    /// person is ordinary, and nothing yet needs to rank them.
    /// </remarks>
    public ContactRoleAssignment AddRole(ContactRoleId roleId)
    {
        if (_roles.Any(x => x.RoleId == roleId))
            throw new BusinessRuleViolationException(
                ContactErrors.RoleAlreadyHeld);

        var assignment = new ContactRoleAssignment(
            ContactRoleAssignmentId.New(), roleId);

        _roles.Add(assignment);

        return assignment;
    }

    public void RemoveRole(ContactRoleId roleId)
    {
        var assignment = _roles.FirstOrDefault(x => x.RoleId == roleId)
            ?? throw new NotFoundException(ContactErrors.RoleNotHeld);

        _roles.Remove(assignment);
    }

    /// <summary>
    /// Adds an address this person is reachable at. No <em>preferred</em> flag:
    /// several addresses is ordinary, and nothing yet needs to choose between
    /// them.
    /// </summary>
    public ContactEmail AddEmail(string address)
    {
        var email = new ContactEmail(ContactEmailId.New(), address);

        if (_emails.Any(x => string.Equals(
                x.Address, email.Address, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessRuleViolationException(
                ContactErrors.EmailAlreadyRecorded);
        }

        _emails.Add(email);

        return email;
    }

    /// <param name="kind">
    /// Office, fax or mobile — optional, because the aggregate cannot invent an
    /// answer nobody gave. A caller that knows should say; a caller that does
    /// not must not guess on the user's behalf.
    /// </param>
    public ContactPhone AddPhone(string number, ContactPhoneKind? kind = null)
    {
        var phone = new ContactPhone(ContactPhoneId.New(), number, kind);

        if (_phones.Any(x => x.Number == phone.Number))
            throw new BusinessRuleViolationException(
                ContactErrors.PhoneAlreadyRecorded);

        _phones.Add(phone);

        return phone;
    }

    public void Rename(string firstName, string lastName)
    {
        FirstName = Name(firstName, ContactErrors.FirstNameRequired);
        LastName = Name(lastName, ContactErrors.LastNameRequired);
    }

    /// <summary>
    /// Retires the contact. Everything that references them is untouched — "do
    /// not name this person on anything new", not "pretend they never existed".
    /// </summary>
    public void Deactivate(DateOnly on)
    {
        if (Status == OrganizationStatus.Inactive)
            throw new BusinessRuleViolationException(
                ContactErrors.AlreadyInactive);

        Status = OrganizationStatus.Inactive;
        StatusDate = Dated(on);
    }

    public void Activate(DateOnly on)
    {
        if (Status == OrganizationStatus.Active)
            throw new BusinessRuleViolationException(
                ContactErrors.AlreadyActive);

        Status = OrganizationStatus.Active;
        StatusDate = Dated(on);
    }

    private static DateOnly Dated(DateOnly on)
        => on == default
            ? throw new DomainException(ContactErrors.StatusDateRequired)
            : on;

    private static string Name(string? value, string missing)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(missing);

        var trimmed = value.Trim();

        return trimmed.Length > NameMaxLength
            ? throw new DomainException(ContactErrors.NameTooLong)
            : trimmed;
    }

    private static string? Trimmed(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
