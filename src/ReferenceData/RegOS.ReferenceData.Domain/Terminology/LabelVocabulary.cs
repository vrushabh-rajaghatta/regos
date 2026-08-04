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

    /// <summary>
    /// What a market's own controlled labelling document is.
    /// </summary>
    /// <remarks>
    /// <b>Carton artwork is in this list rather than in an aggregate of its
    /// own</b>, and that is EPIC-018 D2. A printed carton is a controlled
    /// document an authority approved, revised on its own history and derived
    /// from a core position — which is what every other entry here is.
    /// <para>
    /// It stays terminology only while <em>every invariant applies equally to
    /// every type</em>. When the domain starts reading
    /// <c>if (Type == Artwork)</c>, artwork has become a different thing and
    /// belongs in its own root. <c>LocalLabelTypeBranchTests</c> counts those
    /// branches so the question is asked by the build rather than by whoever
    /// happens to be reading a year from now.
    /// </para>
    /// </remarks>
    public static IReadOnlyList<CodedConcept> LocalLabelTypes { get; } =
    [
        // What the prescriber reads, as approved in this market.
        CodedConcept.Internal("SMPC", "Prescribing information"),

        // What the patient reads.
        CodedConcept.Internal("PIL", "Patient information leaflet"),

        // The printed carton. A controlled document like the rest of them.
        CodedConcept.Internal("ARTWORK", "Carton artwork"),

        // The immediate container: vial label, blister foil, ampoule.
        CodedConcept.Internal("CONTAINER", "Container label")
    ];

    public static CodedConcept? GlobalLabelTypeOf(string? code)
        => CodedConceptLookup.Find(GlobalLabelTypes, code);

    public static CodedConcept? LocalLabelTypeOf(string? code)
        => CodedConceptLookup.Find(LocalLabelTypes, code);
}
