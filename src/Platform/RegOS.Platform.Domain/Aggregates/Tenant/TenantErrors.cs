namespace RegOS.Platform.Domain.Aggregates.Tenant;

public static class TenantErrors
{
    public const string NameRequired = "Tenant name is required.";

    public const string AlreadyActive = "Tenant is already active.";

    public const string AlreadyInactive = "Tenant is already inactive.";
}
