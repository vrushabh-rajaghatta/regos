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

    public const string EmailAlreadyInUse =
        "A user with this email already exists in the organization.";
}
