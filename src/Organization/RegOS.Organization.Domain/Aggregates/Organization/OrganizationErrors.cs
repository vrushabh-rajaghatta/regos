namespace RegOS.Organization.Domain.Aggregates.Organization;

public static class OrganizationErrors
{
    public const string LegalNameRequired =
        "Organization legal name is required.";

    public const string TypeInvalid =
        "Organization type is not a recognized value.";

    public const string AlreadyInactive =
        "Organization is already inactive.";

    public const string NotFound =
        "Organization not found.";
}
