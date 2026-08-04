namespace RegOS.Labeling.Domain.Aggregates.GlobalLabels;

public static class GlobalLabelErrors
{
    public const string TenantRequired =
        "A label must belong to a tenant.";

    public const string GlobalProductRequired =
        "A label must be held for a product.";

    public const string NameRequired =
        "A label needs a name.";

    public static readonly string NameTooLong =
        $"A label name must be {GlobalLabel.NameMaxLength} characters or fewer.";

    public const string LabelTypeRequired =
        "A label type is required.";

    public const string LabelTypeNotRecognised =
        "That label type is not recognised.";

    public const string NoOpenDraft =
        "This label has no open draft.";

    public const string VersionNotFound =
        "That version does not belong to this label.";

    public const string DraftAlreadyOpen =
        "This label already has an open draft. Publish it, or change it, "
        + "before starting another.";

    public const string VersionNotDraft =
        "A published version is frozen. Start a new draft to change it.";

    public const string EffectiveFromRequired =
        "The date this version takes effect is required.";

    /// <summary>
    /// The invariant that makes the content link worth having. A core data
    /// sheet version is the document; publishing one without it would record a
    /// version number and nothing a person could read.
    /// </summary>
    public const string ContentRequiredToPublish =
        "Attach the label document before publishing this version.";

    public const string EffectiveFromNotAfterVersionInForce =
        "A version takes effect after the one it replaces, not before or on "
        + "the same day.";

    public static readonly string ChangeSummaryTooLong =
        $"A change summary must be {GlobalLabelVersion.ChangeSummaryMaxLength} "
        + "characters or fewer.";
}
