namespace RegOS.Organization.Domain.Aggregates.Organization;

public static class OrganizationErrors
{
    public const string LegalNameRequired =
        "Organization legal name is required.";

    public const string TypeInvalid =
        "Organization type is not a recognized value.";

    public const string AlreadyInactive =
        "Organization is already inactive.";

    public const string AlreadyActive =
        "Organization is already active.";

    public const string NotFound =
        "Organization not found.";

    public const string IdentifierSchemeRequired =
        "An identifier must name the scheme that issued it.";

    public const string IdentifierValueRequired =
        "An identifier needs a value.";

    public const string IdentifierValueTooLong =
        "The identifier value is too long.";

    public const string IdentifierSchemeAlreadyRecorded =
        "This organization already has an identifier from that scheme.";

    public const string IdentifierNotFound =
        "This organization has no such identifier.";
}
