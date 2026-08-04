namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// The kinds of label a company holds centrally — the documents a core data
/// sheet family is made of.
/// </summary>
/// <remarks>
/// <b>A label type is terminology, not a domain type.</b> Nothing in
/// <c>Labeling</c> branches on it: a core safety information document versions,
/// publishes and supersedes exactly as a company core data sheet does. That is
/// the test that keeps it a <see cref="CodedConcept"/> rather than an
/// <c>enum</c> — the same test that made <c>IngredientRole</c> an enum and dose
/// form a concept (ADR-058 §3).
/// <para>
/// Every entry is <see cref="CodingSystems.RegosInternal"/>. These are the
/// industry's ordinary words rather than a governed code list — there is no
/// registry of label types to be authoritative about, which is a different
/// situation from EDQM dose forms and worth not conflating with it.
/// </para>
/// </remarks>
public static class LabelVocabulary
{
    /// <summary>What a globally-held label document is.</summary>
    public static IReadOnlyList<CodedConcept> GlobalLabelTypes { get; } =
    [
        // The document the whole family hangs from — safety, indications and
        // dosing as the company holds them, before any market localises them.
        CodedConcept.Internal("CCDS", "Company Core Data Sheet"),

        // The safety section held separately, which many companies govern on
        // its own cycle because pharmacovigilance drives it.
        CodedConcept.Internal("CSI", "Core Safety Information"),

        CodedConcept.Internal("CPI", "Core Prescribing Information"),

        // What the patient reads, as against what the prescriber reads.
        CodedConcept.Internal("CPIL", "Core Patient Information Leaflet")
    ];

    public static CodedConcept? GlobalLabelTypeOf(string? code)
        => CodedConceptLookup.Find(GlobalLabelTypes, code);
}
