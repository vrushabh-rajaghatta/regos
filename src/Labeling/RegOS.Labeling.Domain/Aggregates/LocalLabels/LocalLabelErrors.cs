namespace RegOS.Labeling.Domain.Aggregates.LocalLabels;

public static class LocalLabelErrors
{
    public const string TenantRequired =
        "A local label must belong to a tenant.";

    public const string MedicinalProductRequired =
        "A local label must be held for a market.";

    public const string LabelTypeRequired =
        "A label type is required.";

    public const string LabelTypeNotRecognised =
        "That label type is not recognised.";

    public const string LanguageRequired =
        "A language is required.";

    public const string RevisionNotFound =
        "That revision does not belong to this label.";

    public const string NoOpenDraft =
        "This label has no open draft.";

    public const string DraftAlreadyOpen =
        "This label already has an open draft. Approve it, or discard it, "
        + "before starting another.";

    /// <summary>
    /// An approved labelling document is a controlled record. Overwriting one
    /// is a governance failure rather than an edit, which is why the refusal
    /// says what to do instead.
    /// </summary>
    public const string RevisionNotDraft =
        "An approved revision cannot be changed. Start a new revision instead.";

    public const string ContentRequiredToPublish =
        "Attach the approved document before putting this revision in force.";

    public const string ApprovedOnRequired =
        "The date the authority approved this revision is required.";

    public const string EffectiveFromRequired =
        "The date this revision takes effect is required.";

    public const string EffectiveBeforeApproval =
        "A revision cannot take effect before the authority approved it.";

    public const string EffectiveFromNotAfterRevisionInForce =
        "A revision takes effect after the one it replaces, not before or on "
        + "the same day.";

    public static readonly string ChangeSummaryTooLong =
        $"A change summary must be {LocalLabelRevision.ChangeSummaryMaxLength} "
        + "characters or fewer.";

    public static readonly string DataCarrierCodeTooLong =
        $"A data-carrier code must be "
        + $"{LocalLabelRevision.DataCarrierCodeMaxLength} characters or fewer.";
}
