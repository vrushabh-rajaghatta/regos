namespace RegOS.Product.Domain.Product;

public static class PackageItemErrors
{
    public const string NotFound =
        "That pack layer does not exist.";

    public const string TenantRequired =
        "A pack layer must belong to a tenant.";

    public const string PackRequired =
        "A pack layer must belong to a pack.";

    public const string QuantityMustBePositive =
        "How many of these are there? A pack layer holds at least one.";

    public const string DescriptionTooLong =
        "The description is too long.";

    /// <remarks>
    /// Names the layer, not the field: a parent from another pack is a
    /// different mistake from one that does not exist at all, and both arrive
    /// here.
    /// </remarks>
    public const string ParentNotFound =
        "That layer is not part of this pack.";

    public const string TooDeep =
        "A pack may be four layers deep. Nothing demonstrated needs more.";

    public const string WouldBeItsOwnAncestor =
        "A layer cannot be placed inside itself.";

    public const string StillHoldsItems =
        "Empty this layer before removing it.";
}
