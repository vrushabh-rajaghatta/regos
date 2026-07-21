namespace RegOS.Platform.Domain.Aggregates.User;

public static class UserErrors
{
    /// <summary>
    /// An invited user becomes active by accepting their invitation, never by
    /// an administrator activating them — that path was the only way to reach
    /// Active without a credential, and ADR-027 closed it.
    /// </summary>
    public const string OnlyInactiveUsersCanBeActivated =
        "Only a deactivated user can be activated. An invited user becomes "
            + "active by accepting their invitation.";

    public const string OnlyInvitedUsersCanBeReinvited =
        "Only a user who has not yet accepted can be re-invited.";

    public const string TenantRequired =
        "A user must belong to a tenant.";

    public const string EmailRequired =
        "A user email is required.";

    public const string FirstNameRequired =
        "First name is required.";

    public const string LastNameRequired =
        "Last name is required.";
}
