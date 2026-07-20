using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Domain.Aggregates.User;

/// <summary>
/// A person who can access RegOS on behalf of an <see cref="OrganizationId"/>.
/// This is the business concept of a person — not an authentication account;
/// passwords, roles, permissions and sign-in are separate concerns owned
/// elsewhere.
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    private User(
        UserId id,
        OrganizationId organizationId,
        Email email,
        string firstName,
        string lastName,
        DateTime createdOn)
    {
        Id = id;
        OrganizationId = organizationId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Status = UserStatus.Invited;
        CreatedOn = createdOn;
    }

    public OrganizationId OrganizationId { get; private set; }

    public Email Email { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public UserStatus Status { get; private set; }

    public DateTime CreatedOn { get; private set; }

    /// <summary>
    /// Invites a new user: creates them in the <see cref="UserStatus.Invited"/>
    /// state, pending activation. The Email value object already guarantees a
    /// valid, normalized address; the aggregate enforces the remaining
    /// invariants (organization present, names supplied).
    /// </summary>
    public static User Create(
        OrganizationId organizationId,
        Email email,
        string firstName,
        string lastName)
    {
        if (organizationId is null)
            throw new DomainException(UserErrors.OrganizationRequired);

        if (email is null)
            throw new DomainException(UserErrors.EmailRequired);

        return new User(
            UserId.New(),
            organizationId,
            email,
            RequireName(firstName, UserErrors.FirstNameRequired),
            RequireName(lastName, UserErrors.LastNameRequired),
            DateTime.UtcNow);
    }

    /// <summary>Updates the user's name. No-op when the name is unchanged.</summary>
    public void ChangeName(string firstName, string lastName)
    {
        var newFirstName = RequireName(firstName, UserErrors.FirstNameRequired);
        var newLastName = RequireName(lastName, UserErrors.LastNameRequired);

        if (newFirstName == FirstName && newLastName == LastName)
            return;

        FirstName = newFirstName;
        LastName = newLastName;
    }

    /// <summary>Changes the user's email. No-op when the email is unchanged.</summary>
    public void ChangeEmail(Email email)
    {
        if (email is null)
            throw new DomainException(UserErrors.EmailRequired);

        if (email == Email)
            return;

        Email = email;
    }

    /// <summary>Invited/Inactive -> Active. Idempotent when already active.</summary>
    public void Activate()
    {
        if (Status == UserStatus.Active)
            return;

        Status = UserStatus.Active;
    }

    /// <summary>Active/Invited -> Inactive. Idempotent when already inactive.</summary>
    public void Deactivate()
    {
        if (Status == UserStatus.Inactive)
            return;

        Status = UserStatus.Inactive;
    }

    private static string RequireName(string value, string error)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(error);

        return value.Trim();
    }
}
