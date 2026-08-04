namespace RegOS.Labeling.Application;

/// <summary>
/// Refusals that belong to no single aggregate — the checks a handler makes
/// about another context's records before letting one be named.
/// </summary>
public static class LabelingRuleErrors
{
    public const string GlobalLabelDoesNotExist =
        "Label does not exist.";

    public const string GlobalProductDoesNotExist =
        "Product does not exist.";

    /// <summary>
    /// <b>The anti-corruption check that keeps ADR-059 §6 honest.</b> Labeling
    /// may point at a document; it may not point at <em>any</em> document. A
    /// label held for product A whose content is a file belonging to product B
    /// is a record that reads correctly and is wrong, and nothing downstream
    /// would ever notice.
    /// </summary>
    public const string ContentBelongsToAnotherProduct =
        "That document belongs to a different product.";

    public const string ContentDoesNotExist =
        "Document does not exist.";
}
