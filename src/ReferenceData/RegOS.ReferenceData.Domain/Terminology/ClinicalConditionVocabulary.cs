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
        CodedConcept.Internal("BACT-INF", "Bacterial infection"),

        // Added in S004, because a contraindication was not expressible without
        // them. The commonest contraindication in pharmaceutical labelling is
        // hypersensitivity to the product itself, and a vocabulary that could
        // not say it would have made the aggregate look usable and not be.
        CodedConcept.Internal("HYPERSENS-AS", "Hypersensitivity to the active substance"),
        CodedConcept.Internal("HYPERSENS-EX", "Hypersensitivity to any excipient"),
        CodedConcept.Internal("SEVERE-RENAL", "Severe renal impairment"),
        CodedConcept.Internal("SEVERE-HEPATIC", "Severe hepatic impairment"),
        CodedConcept.Internal("NAUSEA", "Nausea"),
        CodedConcept.Internal("HEADACHE", "Headache"),
        CodedConcept.Internal("DIARRHOEA", "Diarrhoea"),
        CodedConcept.Internal("ANAPHYLAXIS", "Anaphylactic reaction")
    ];

    /// <summary>
    /// How often an undesirable effect occurs, in the bands a summary of
    /// product characteristics uses.
    /// </summary>
    /// <remarks>
    /// <b>On <c>UndesirableEffect</c> alone.</b> Nothing else carries it, and
    /// nothing branches on it — it is a coded clinical concept, orthogonal to
    /// the population a statement applies to, and it is the one attribute S004
    /// found that the three statement types do not share.
    /// <para>
    /// The bands are the industry's ordinary ones. The thresholds behind them
    /// (≥1/10, ≥1/100, …) are deliberately not modelled: RegOS records what the
    /// label says, and a computed frequency would be RegOS asserting a
    /// calculation it did not perform.
    /// </para>
    /// </remarks>
    public static IReadOnlyList<CodedConcept> Frequencies { get; } =
    [
        CodedConcept.Internal("VERY-COMMON", "Very common"),
        CodedConcept.Internal("COMMON", "Common"),
        CodedConcept.Internal("UNCOMMON", "Uncommon"),
        CodedConcept.Internal("RARE", "Rare"),
        CodedConcept.Internal("VERY-RARE", "Very rare"),
        CodedConcept.Internal("NOT-KNOWN", "Not known")
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

    /// <summary>What kind of thing this product interacts with.</summary>
    /// <remarks>
    /// Not derivable from the interactant, which is why it is recorded. "St
    /// John's wort" is a herbal product and a CYP3A4 inducer; which of those a
    /// label means is the label's statement, not ours to infer.
    /// </remarks>
    public static IReadOnlyList<CodedConcept> InteractionTypes { get; } =
    [
        CodedConcept.Internal("DRUG-DRUG", "With another medicine"),
        CodedConcept.Internal("DRUG-FOOD", "With food or drink"),
        CodedConcept.Internal("DRUG-DISEASE", "With a condition"),
        CodedConcept.Internal("DRUG-LAB", "With a laboratory test")
    ];

    /// <summary>
    /// How much the interaction matters clinically, as the label states it.
    /// </summary>
    /// <remarks>
    /// Nullable on the aggregate: many labels describe an interaction and its
    /// management without grading it, and inventing a grade would be RegOS
    /// asserting a clinical judgement nobody made.
    /// </remarks>
    public static IReadOnlyList<CodedConcept> InteractionSeverities { get; } =
    [
        CodedConcept.Internal("CONTRAINDICATED", "Contraindicated"),
        CodedConcept.Internal("MAJOR", "Major"),
        CodedConcept.Internal("MODERATE", "Moderate"),
        CodedConcept.Internal("MINOR", "Minor")
    ];

    public static CodedConcept? InteractionTypeOf(string? code)
        => CodedConceptLookup.Find(InteractionTypes, code);

    public static CodedConcept? InteractionSeverityOf(string? code)
        => CodedConceptLookup.Find(InteractionSeverities, code);

    public static CodedConcept? ConditionOf(string? code)
        => CodedConceptLookup.Find(Conditions, code);

    public static CodedConcept? FrequencyOf(string? code)
        => CodedConceptLookup.Find(Frequencies, code);

    public static CodedConcept? PhysiologicalConditionOf(string? code)
        => CodedConceptLookup.Find(PhysiologicalConditions, code);

    public static CodedConcept? GenderOf(string? code)
        => CodedConceptLookup.Find(Genders, code);

    public static CodedConcept? AgeUnitOf(string? code)
        => CodedConceptLookup.Find(AgeUnits, code);

    public static CodedConcept? TherapyRelationshipOf(string? code)
        => CodedConceptLookup.Find(TherapyRelationships, code);
}
