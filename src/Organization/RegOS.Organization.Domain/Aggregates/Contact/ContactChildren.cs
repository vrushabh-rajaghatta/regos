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
/// What kind of line a number reaches. <b>A business concept, not a wire
/// vocabulary</b> — the question *"is that your office number, your fax or your
/// mobile?"* is the obvious next one to ask about any number, and it would be
/// worth answering if no authority had ever asked.
/// </summary>
/// <remarks>
/// FDA's <c>telephone-number-type</c> happens to enumerate exactly these three
/// (<c>fdatnt1</c>, <c>fdatnt2</c>, <c>fdatnt3</c> — evidence E30), and the
/// coincidence is the world's rather than the wire's. **The translation belongs
/// at the boundary**, in the renderer, the way ADR-048 keeps contact roles out
/// of the format's taxonomy. Nothing here carries a token.
/// </remarks>
public enum ContactPhoneKind
{
    Business = 1,
    Fax = 2,
    Mobile = 3,
}

/// <summary>
/// One number this person is reachable on. Free text: international formats
/// vary too much for RegOS to have an opinion, and normalising would lose the
/// extension a user typed.
/// </summary>
public sealed class ContactPhone : Entity<ContactPhoneId>
{
    internal ContactPhone(
        ContactPhoneId id, string number, ContactPhoneKind? kind)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException(ContactErrors.PhoneRequired);

        var trimmed = number.Trim();

        if (trimmed.Length > Contact.PhoneMaxLength)
            throw new DomainException(ContactErrors.PhoneTooLong);

        if (kind is { } declared && !Enum.IsDefined(declared))
            throw new DomainException(ContactErrors.PhoneKindUnknown);

        Id = id;
        Number = trimmed;
        Kind = kind;
    }

    private ContactPhone()
    {
    }

    public string Number { get; private set; } = default!;

    /// <summary>
    /// <b>Null means <em>recorded before RegOS asked</em></b>, not "unknown" and
    /// not "assume business".
    /// </summary>
    /// <remarks>
    /// Every phone stored before 2026-08-03 has one, because nobody was ever
    /// offered the question — the same shape as
    /// <c>Submission.SubmissionSubTypeId</c>, where a null marks a row that
    /// predates the model rather than a value someone declined to give.
    /// <para>
    /// A null is a gap someone using the system can close by answering. That is
    /// what makes it a <b>data-completeness</b> refusal at generation time —
    /// the same class as a missing application number — rather than a refusal
    /// about our history or the authority's vocabulary.
    /// </para>
    /// </remarks>
    public ContactPhoneKind? Kind { get; private set; }
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

    public const string PhoneKindUnknown =
        "A phone number is an office line, a fax or a mobile — or it is left "
        + "unsaid.";

    public const string PhoneAlreadyRecorded =
        "This contact already has that phone number.";

    public const string AlreadyInactive = "This contact is already inactive.";

    public const string AlreadyActive = "This contact is already active.";
}
