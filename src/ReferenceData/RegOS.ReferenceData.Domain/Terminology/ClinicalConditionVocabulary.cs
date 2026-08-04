namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// What a product may be approved to treat, prevent or diagnose — the clinical
/// concept, as against the words a label uses for it.
/// </summary>
/// <remarks>
/// <b>Named for what it is responsible for, not for everything clinical.</b>
/// Contraindications, adverse reactions and physiological conditions are not
/// obviously the same list, and a type called <c>ClinicalVocabulary</c> would
/// have been stretched to hold them by whoever needed one first. Each arrives
/// with its own name when it arrives.
/// <para>
/// <b>A code, not free text, and that is the whole point.</b> <em>Type II
/// diabetes mellitus</em>, <em>Diabète sucré de type 2</em> and <em>Diabetes
/// mellitus Typ 2</em> are one clinical concept in three markets, and only a
/// code can say so. <em>"Which markets approve indication X?"</em> does not
/// exist as a question over free text — the same argument
/// [ADR-058](../../../../docs/adr/ADR-058-substances-are-shared-facts-ingredients-are-roles.md)
/// §1 made for splitting <c>Substance</c> from <c>Ingredient</c>, one epic
/// later.
/// </para>
/// <para>
/// <b>Demonstration data only.</b> These records intentionally do not represent
/// MedDRA, SNOMED CT, ICD or any other licensed clinical terminology. RegOS
/// holds no licence for one, and every entry is
/// <see cref="CodingSystems.RegosInternal"/>. The list exists to exercise the
/// model, not to represent clinical practice: nobody's real indication is in it.
/// Licensed terminology is introduced separately, and the <c>System</c> field is
/// what makes that a data migration rather than a redesign (ADR-058 §6).
/// </para>
/// </remarks>
public static class ClinicalConditionVocabulary
{
    /// <summary>A deliberately tiny, unmistakably illustrative set.</summary>
    public static IReadOnlyList<CodedConcept> Conditions { get; } =
    [
        CodedConcept.Internal("T2DM", "Type 2 diabetes mellitus"),
        CodedConcept.Internal("HTN", "Hypertension"),
        CodedConcept.Internal("RA", "Rheumatoid arthritis"),
        CodedConcept.Internal("ASTHMA", "Asthma"),
        CodedConcept.Internal("MDD", "Major depressive disorder"),
        CodedConcept.Internal("EPILEPSY", "Epilepsy"),
        CodedConcept.Internal("PAIN-MOD", "Moderate to severe pain"),
        CodedConcept.Internal("BACT-INF", "Bacterial infection")
    ];

    /// <summary>
    /// Who the statement applies to, physiologically — pregnancy, impairment,
    /// and the states a label routinely qualifies a population by.
    /// </summary>
    public static IReadOnlyList<CodedConcept> PhysiologicalConditions { get; } =
    [
        CodedConcept.Internal("PREGNANCY", "Pregnancy"),
        CodedConcept.Internal("LACTATION", "Breastfeeding"),
        CodedConcept.Internal("RENAL-IMP", "Renal impairment"),
        CodedConcept.Internal("HEPATIC-IMP", "Hepatic impairment")
    ];

    /// <summary>
    /// Terminology rather than an enum: nothing branches on it, and a label that
    /// says "women of childbearing potential" is making a clinical statement
    /// rather than selecting a database value.
    /// </summary>
    public static IReadOnlyList<CodedConcept> Genders { get; } =
    [
        CodedConcept.Internal("ALL", "Any"),
        CodedConcept.Internal("FEMALE", "Female"),
        CodedConcept.Internal("MALE", "Male")
    ];

    /// <summary>How an age boundary is counted.</summary>
    public static IReadOnlyList<CodedConcept> AgeUnits { get; } =
    [
        CodedConcept.Internal("DAY", "days"),
        CodedConcept.Internal("MONTH", "months"),
        CodedConcept.Internal("YEAR", "years")
    ];

    /// <summary>
    /// How one statement relates to another therapy — <em>in combination
    /// with</em>, <em>after failure of</em>, <em>as an adjunct to</em>.
    /// </summary>
    public static IReadOnlyList<CodedConcept> TherapyRelationships { get; } =
    [
        CodedConcept.Internal("COMBINATION", "In combination with"),
        CodedConcept.Internal("ADJUNCT", "As an adjunct to"),
        CodedConcept.Internal("AFTER-FAILURE", "After failure of"),
        CodedConcept.Internal("ALTERNATIVE", "As an alternative to")
    ];

    public static CodedConcept? ConditionOf(string? code)
        => CodedConceptLookup.Find(Conditions, code);

    public static CodedConcept? PhysiologicalConditionOf(string? code)
        => CodedConceptLookup.Find(PhysiologicalConditions, code);

    public static CodedConcept? GenderOf(string? code)
        => CodedConceptLookup.Find(Genders, code);

    public static CodedConcept? AgeUnitOf(string? code)
        => CodedConceptLookup.Find(AgeUnits, code);

    public static CodedConcept? TherapyRelationshipOf(string? code)
        => CodedConceptLookup.Find(TherapyRelationships, code);
}
