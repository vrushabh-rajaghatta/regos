namespace RegOS.Product.Domain.Product;

public static class IngredientErrors
{
    public const string SubstanceRequired =
        "An ingredient must name a substance.";

    /// <remarks>
    /// Says why rather than restating the rule. A user who left the strength
    /// blank needs to know that actives are the part a formulation is
    /// quantified by, not that a field is required.
    /// </remarks>
    public const string ActiveNeedsAStrength =
        "An active ingredient must declare a strength — it is what the product "
        + "is dosed by. An excipient may leave it blank.";

    /// <remarks>
    /// The rule holds over the composition, not over one row, which is why the
    /// message talks about the composition.
    /// </remarks>
    public const string CompositionNeedsAnActive =
        "A composition must contain at least one active ingredient. "
        + "Add the active before removing this one.";

    public const string SubstanceAlreadyInComposition =
        "That substance is already in this composition. "
        + "Correct the existing entry rather than adding a second.";

    public const string NotFound =
        "Ingredient does not exist.";
}
