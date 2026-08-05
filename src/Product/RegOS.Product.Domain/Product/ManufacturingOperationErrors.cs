namespace RegOS.Product.Domain.Product;

public static class ManufacturingOperationErrors
{
    public const string NotFound =
        "That manufacturing operation does not exist.";

    public const string MarketNotFound =
        "That market does not exist.";

    public const string TenantRequired =
        "A manufacturing operation must belong to a tenant.";

    public const string MarketRequired =
        "Name the market this operation is performed for.";

    public const string SiteRequired =
        "Name the site that performs this operation.";

    public const string OperationRequired =
        "Say what the site does — manufacture, package, test or release.";

    /// <remarks>
    /// Named after the ambiguity rather than the field: an operation with no
    /// start date cannot be compared against a licence that approved a site on
    /// a particular day, which is the whole question this aggregate exists for.
    /// </remarks>
    public const string EffectiveFromRequired =
        "Say when this site started performing the operation.";

    public const string CeasedBeforeItStarted =
        "An operation cannot stop before it starts.";

    /// <remarks>
    /// <b>Current, not ever.</b> The same site may perform the same operation
    /// twice over two separate periods — a transfer away and back is ordinary —
    /// so this refuses only a second <em>open</em> period.
    /// </remarks>
    public const string AlreadyPerformedHere =
        "That site already performs this operation for this market. Close the "
        + "existing period before opening another.";

    public const string AlreadyCeased =
        "That operation has already been closed.";

    public const string SiteBelongsToAnotherTenant =
        "That site is not in this tenant's registry.";
}
