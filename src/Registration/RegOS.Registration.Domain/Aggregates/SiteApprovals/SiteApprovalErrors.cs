namespace RegOS.Registration.Domain.Aggregates.SiteApprovals;

public static class SiteApprovalErrors
{
    public const string NotFound =
        "That site approval does not exist.";

    public const string TenantRequired =
        "A site approval must belong to a tenant.";

    public const string SiteRequired =
        "Name the site this licence approves.";

    /// <remarks>
    /// The fact a foreign key could not carry, and the second time this project
    /// has needed it: a licence granted in 2021 that added a packaging site in
    /// 2024 by variation has two dates, and only one of them is the
    /// registration's.
    /// </remarks>
    public const string ApprovedOnRequired =
        "Say when this site was added to the licence — it is often later than "
        + "the licence itself.";

    public const string AlreadyApproved =
        "This licence already approves that site.";

    public const string RegistrationDoesNotExist =
        "That registration does not exist.";

    public const string SiteDoesNotExist =
        "That site is not in this tenant's registry.";
}
