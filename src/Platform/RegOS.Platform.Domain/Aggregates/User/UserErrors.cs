namespace RegOS.Platform.Domain.Aggregates.User;

public static class UserErrors
{
    public const string OrganizationRequired =
        "A user must belong to an organization.";

    public const string EmailRequired =
        "A user email is required.";

    public const string FirstNameRequired =
        "First name is required.";

    public const string LastNameRequired =
        "Last name is required.";
}
