namespace RegOS.Labeling.Domain.Aggregates.Indications;

public static class IndicationErrors
{
    public const string TenantRequired =
        "An indication must belong to a tenant.";

    public const string MedicinalProductRequired =
        "An indication must be approved for a market.";

    public const string ConditionRequired =
        "A clinical condition is required.";

    /// <summary>
    /// RegOS ships a demonstration vocabulary, not MedDRA or SNOMED. The
    /// refusal says which list was consulted so a user does not read it as
    /// "that condition does not exist".
    /// </summary>
    public const string ConditionNotRecognised =
        "That condition is not in the RegOS clinical vocabulary.";

    public const string LabelTextRequired =
        "The wording the label uses is required.";

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

    public const string PopulationNotFound =
        "That population does not belong to this indication.";

    public const string OtherTherapyNotFound =
        "That therapy does not belong to this indication.";

    public const string AgeCannotBeNegative =
        "An age cannot be negative.";

    public const string AgeRangeInverted =
        "An age range runs from the lower bound to the upper one.";

    public const string AgeUnitRequired =
        "An age needs a unit — 2 to 12 could be months or years.";

    public const string AgeUnitWithoutRange =
        "An age unit says nothing without an age.";

    public const string AgeUnitNotRecognised =
        "That age unit is not recognised.";

    public const string GenderNotRecognised =
        "That gender is not recognised.";

    public const string PhysiologicalConditionNotRecognised =
        "That physiological condition is not recognised.";

    public static readonly string DescriptionTooLong =
        $"A description must be {Population.DescriptionMaxLength} characters or fewer.";

    public const string TherapyRequired =
        "The other therapy is required.";

    public static readonly string TherapyTooLong =
        $"A therapy must be {OtherTherapy.TherapyMaxLength} characters or fewer.";

    public const string TherapyRelationshipNotRecognised =
        "That relationship is not recognised.";
}
