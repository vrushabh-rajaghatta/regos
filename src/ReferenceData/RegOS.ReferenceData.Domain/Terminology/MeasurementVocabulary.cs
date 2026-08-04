namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// The units a quantity may be measured in — mass, volume, and the activity
/// units biologicals are expressed in.
/// </summary>
/// <remarks>
/// <b>Deliberately separate from <see cref="PharmaceuticalVocabulary.UnitsOfPresentation"/>,
/// and that separation is the decision.</b> A unit of presentation counts
/// articles — a vial, a tablet; a measurement unit measures quantity — mg, mL,
/// IU. Letting a strength draw its denominator from the presentation list would
/// make <em>"500 mg per tablet"</em> expressible, and that sentence repeats
/// something the presentation already says.
/// <para>
/// <b>A point strength has no denominator at all.</b> <c>500 mg</c> in a
/// presentation whose dose form is <em>Tablet</em> already means <em>500 mg per
/// tablet</em> — the reader composes the two. The denominator exists for
/// concentrations, where the volume is genuinely part of the strength:
/// <c>10 mg / 1 mL</c>. Keeping the two vocabularies apart is what stops a
/// formulation being stated twice and disagreeing with itself.
/// </para>
/// <para>
/// Every entry is <see cref="CodingSystems.RegosInternal"/>. Units are UCUM's
/// territory in the real world, and RegOS does not ship UCUM (ADR-058 §6).
/// </para>
/// </remarks>
public static class MeasurementVocabulary
{
    /// <summary>Mass, volume, and biological activity.</summary>
    public static IReadOnlyList<CodedConcept> Units { get; } =
    [
        // Mass.
        CodedConcept.Internal("MCG", "microgram"),
        CodedConcept.Internal("MG", "mg"),
        CodedConcept.Internal("G", "g"),

        // Volume.
        CodedConcept.Internal("ML", "mL"),
        CodedConcept.Internal("L", "L"),

        // Activity — how biologicals are expressed, and why a strength cannot
        // assume it is measuring mass.
        CodedConcept.Internal("IU", "IU"),
        CodedConcept.Internal("UNIT", "unit"),

        // Ratios, for topicals stated as a percentage.
        CodedConcept.Internal("PERCENT", "%"),

        // Amount of substance.
        CodedConcept.Internal("MMOL", "mmol"),
        CodedConcept.Internal("MOL", "mol")
    ];

    public static CodedConcept? UnitOf(string? code)
        => CodedConceptLookup.Find(Units, code);
}
