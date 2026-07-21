namespace RegOS.Platform.Application;

/// <summary>
/// Business-rule violation messages surfaced by the platform policies.
/// </summary>
public static class PlatformErrors
{
    public const string OrganizationDoesNotExist =
        "Organization does not exist.";

    public const string OrganizationInactive =
        "Organization is inactive and cannot accept users.";

    public const string UserNotFound =
        "User not found.";

    // Deliberately does not say "in this organization": an email identifies
    // exactly one user across RegOS (ADR-021), and the colliding user may
    // belong to an organization the caller cannot see. The wording states the
    // rule without disclosing where the collision is.
    public const string EmailAlreadyInUse =
        "A user with this email address already exists.";
}
