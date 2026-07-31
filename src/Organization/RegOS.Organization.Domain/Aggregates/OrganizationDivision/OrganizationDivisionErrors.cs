namespace RegOS.Organization.Domain.Aggregates.OrganizationDivision;

public static class OrganizationDivisionErrors
{
    public const string TenantRequired =
        "A division must belong to a tenant.";

    public const string OrganizationRequired =
        "A division must belong to an organization.";

    public const string NameRequired = "A division needs a name.";

    public const string NameTooLong = "The division name is too long.";

    public const string StatusDateRequired =
        "The date the division's status took effect is required.";

    public const string AlreadyInactive = "This division is already inactive.";

    public const string AlreadyActive = "This division is already active.";
}
