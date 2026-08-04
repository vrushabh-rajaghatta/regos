namespace RegOS.Product.Domain.Product;

public static class MedicinalProductComponentErrors
{
    public const string TenantRequired =
        "A component must belong to a tenant.";

    public const string MarketRequired =
        "A component must belong to a market.";

    public const string NameRequired =
        "A component name is required.";

    public static readonly string NameTooLong =
        $"A component name must be "
        + $"{MedicinalProductComponent.NameMaxLength} characters or fewer.";

    public static readonly string DescriptionTooLong =
        $"A description must be "
        + $"{MedicinalProductComponent.DescriptionMaxLength} characters or fewer.";

    public const string ComponentTypeRequired =
        "A component must say what kind of article it is.";

    public const string QuantityMustBePositive =
        "A component's quantity must be greater than zero.";

    public const string NotFound =
        "Component does not exist.";

    public const string ParentNotFound =
        "That component cannot be placed inside something that does not exist.";

    public const string ParentInAnotherMarket =
        "A component cannot be placed inside one from a different market.";

    /// <remarks>
    /// Names the depth rather than saying "too deep" — a user who has hit the
    /// limit needs to know what the limit is to decide whether their model is
    /// wrong or ours is.
    /// </remarks>
    public static readonly string TooDeep =
        $"A component tree may be {ComponentTree.MaxDepth} levels deep. "
        + "A kit holding a vial holding an article is already deeper than "
        + "anything a presentation has needed — if this is genuinely nested "
        + "further, the model is worth revisiting rather than the limit.";

    public const string WouldBeItsOwnAncestor =
        "A component cannot be placed inside itself, or inside anything it "
        + "already contains.";

    /// <remarks>
    /// Refuses rather than cascading. Removing a kit and silently taking its
    /// contents with it is the kind of quiet data loss a regulatory record
    /// should never allow, and emptying it first makes the intent explicit.
    /// </remarks>
    public const string StillHoldsComponents =
        "This component still holds others. Remove what is inside it first.";
}
