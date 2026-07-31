namespace RegOS.Organization.Domain.Aggregates.OrganizationSite;

public static class OrganizationSiteErrors
{
    public const string TenantRequired =
        "A site must belong to a tenant.";

    public const string OrganizationRequired =
        "A site must belong to an organization.";

    public const string NameRequired =
        "A site needs a name.";

    public const string NameTooLong =
        "The site name is too long.";

    public const string TypeInvalid =
        "That is not a site type.";

    public const string CountryRequired =
        "A site address must name a country.";

    public const string AddressLineTooLong =
        "That address line is too long.";

    public const string AddressRequired =
        "A site must have an address.";

    public const string StatusDateRequired =
        "The date the site's status took effect is required.";

    // Identifiers
    public const string IdentifierSchemeRequired =
        "An identifier must name the scheme that issued it.";

    public const string IdentifierValueRequired =
        "An identifier needs a value.";

    public const string IdentifierValueTooLong =
        "The identifier value is too long.";

    public const string IdentifierSchemeAlreadyRecorded =
        "This site already has an identifier from that scheme.";

    public const string IdentifierNotFound =
        "This site has no such identifier.";

    // Activation
    public const string AlreadyInactive =
        "This site is already inactive.";

    public const string AlreadyActive =
        "This site is already active.";
}
