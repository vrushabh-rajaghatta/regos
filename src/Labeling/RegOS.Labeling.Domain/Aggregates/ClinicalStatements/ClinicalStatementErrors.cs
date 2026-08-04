namespace RegOS.Labeling.Domain.Aggregates.ClinicalStatements;

/// <summary>
/// Refusals that belong to the qualifiers rather than to any one statement.
/// </summary>
/// <remarks>
/// Split out of <c>IndicationErrors</c> in S004, when <see cref="Population"/>
/// acquired its second and third owners. The messages did not change — only
/// where they live, so that <c>Contraindication</c> does not have to reach into
/// <c>Indications</c> to say "that age needs a unit".
/// </remarks>
public static class ClinicalStatementErrors
{
    public const string PopulationNotFound =
        "That population does not belong to this statement.";

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

    // --- shared by every statement that names a condition and a wording ------

    public const string TenantRequired =
        "A clinical statement must belong to a tenant.";

    public const string MedicinalProductRequired =
        "A clinical statement must belong to a market.";

    public const string ConditionRequired =
        "A clinical condition is required.";

    /// <summary>
    /// Says which list was consulted, so a user does not read it as "that
    /// condition does not exist". RegOS ships a demonstration vocabulary.
    /// </summary>
    public const string ConditionNotRecognised =
        "That condition is not in the RegOS clinical vocabulary.";

    public const string LabelTextRequired =
        "The wording the label uses is required.";
}
