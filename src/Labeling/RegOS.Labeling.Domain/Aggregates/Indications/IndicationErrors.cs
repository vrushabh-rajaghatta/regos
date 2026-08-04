using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;

namespace RegOS.Labeling.Domain.Aggregates.Indications;

/// <summary>
/// What only an indication can refuse. The qualifier and condition messages it
/// used to hold moved to <see cref="ClinicalStatementErrors"/> in S004, when
/// they acquired a second and third caller.
/// </summary>
public static class IndicationErrors
{
    public static readonly string LabelTextTooLong =
        $"Label text must be {Indication.LabelTextMaxLength} characters or fewer.";

    public const string StatusNotRecognised =
        "That decision is not recognised.";

    public const string OccurredOnRequired =
        "The date the decision took effect is required.";

    public const string OccurredOnBeforePreviousEntry =
        "History is read in business time: a decision cannot take effect before "
        + "the one it follows.";

    public static string AlreadyInStatus(IndicationStatus status)
        => $"This indication is already {status}.";

    public static readonly string NoteTooLong =
        $"A note must be {IndicationStatusEntry.NoteMaxLength} characters or fewer.";


    public const string OtherTherapyNotFound =
        "That therapy does not belong to this indication.";









    public const string TherapyRequired =
        "The other therapy is required.";

    public static readonly string TherapyTooLong =
        $"A therapy must be {OtherTherapy.TherapyMaxLength} characters or fewer.";

    public const string TherapyRelationshipNotRecognised =
        "That relationship is not recognised.";
}
