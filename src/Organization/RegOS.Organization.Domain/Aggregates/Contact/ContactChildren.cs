using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Organization.Domain.Aggregates.Contact;

public sealed class ContactId : StronglyTypedId
{
    public ContactId(Guid value) : base(value)
    {
    }

    public static ContactId New() => new(Guid.NewGuid());

    public static ContactId From(Guid value) => new(value);

    public static implicit operator Guid(ContactId id) => id.Value;
}

public sealed class ContactRoleAssignmentId : StronglyTypedId
{
    public ContactRoleAssignmentId(Guid value) : base(value)
    {
    }

    public static ContactRoleAssignmentId New() => new(Guid.NewGuid());
}

public sealed class ContactEmailId : StronglyTypedId
{
    public ContactEmailId(Guid value) : base(value)
    {
    }

    public static ContactEmailId New() => new(Guid.NewGuid());
}

public sealed class ContactPhoneId : StronglyTypedId
{
    public ContactPhoneId(Guid value) : base(value)
    {
    }

    public static ContactPhoneId New() => new(Guid.NewGuid());
}

/// <summary>One role this person holds. Only the aggregate creates these.</summary>
public sealed class ContactRoleAssignment : Entity<ContactRoleAssignmentId>
{
    internal ContactRoleAssignment(
        ContactRoleAssignmentId id,
        ContactRoleId roleId)
    {
        if (roleId == default)
            throw new DomainException(ContactErrors.RoleRequired);

        Id = id;
        RoleId = roleId;
    }

    private ContactRoleAssignment()
    {
    }

    public ContactRoleId RoleId { get; private set; }
}

/// <summary>
/// One address this person is reachable at. Stored as written — RegOS records
/// what it was told rather than deciding what a valid address is.
/// </summary>
public sealed class ContactEmail : Entity<ContactEmailId>
{
    internal ContactEmail(ContactEmailId id, string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException(ContactErrors.EmailRequired);

        var trimmed = address.Trim();

        if (trimmed.Length > Contact.EmailMaxLength)
            throw new DomainException(ContactErrors.EmailTooLong);

        // The one structural check worth making: without an @ it is not an
        // address at all, and the mistake is almost always a mis-pasted field.
        if (!trimmed.Contains('@'))
            throw new DomainException(ContactErrors.EmailNotAnAddress);

        Id = id;
        Address = trimmed;
    }

    private ContactEmail()
    {
    }

    public string Address { get; private set; } = default!;
}

/// <summary>
/// One number this person is reachable on. Free text: international formats
/// vary too much for RegOS to have an opinion, and normalising would lose the
/// extension a user typed.
/// </summary>
public sealed class ContactPhone : Entity<ContactPhoneId>
{
    internal ContactPhone(ContactPhoneId id, string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException(ContactErrors.PhoneRequired);

        var trimmed = number.Trim();

        if (trimmed.Length > Contact.PhoneMaxLength)
            throw new DomainException(ContactErrors.PhoneTooLong);

        Id = id;
        Number = trimmed;
    }

    private ContactPhone()
    {
    }

    public string Number { get; private set; } = default!;
}

public static class ContactErrors
{
    public const string TenantRequired = "A contact must belong to a tenant.";

    public const string OrganizationRequired =
        "A contact must belong to an organization.";

    public const string FirstNameRequired = "A contact needs a first name.";

    public const string LastNameRequired = "A contact needs a last name.";

    public const string NameTooLong = "That name is too long.";

    public const string StatusDateRequired =
        "The date the contact's status took effect is required.";

    public const string RoleRequired = "A role assignment must name a role.";

    public const string RoleAlreadyHeld = "This contact already holds that role.";

    public const string RoleNotHeld = "This contact does not hold that role.";

    public const string EmailRequired = "An email address cannot be blank.";

    public const string EmailTooLong = "That email address is too long.";

    public const string EmailNotAnAddress = "That is not an email address.";

    public const string EmailAlreadyRecorded =
        "This contact already has that email address.";

    public const string PhoneRequired = "A phone number cannot be blank.";

    public const string PhoneTooLong = "That phone number is too long.";

    public const string PhoneAlreadyRecorded =
        "This contact already has that phone number.";

    public const string AlreadyInactive = "This contact is already inactive.";

    public const string AlreadyActive = "This contact is already active.";
}
