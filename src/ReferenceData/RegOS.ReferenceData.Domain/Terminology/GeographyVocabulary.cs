namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// The words that say which regulatory groupings a country belongs to.
/// </summary>
/// <remarks>
/// <b>The eighth vocabulary, and each still answers one question.</b>
/// <see cref="PharmaceuticalVocabulary"/> — <em>what is this medicine?</em>
/// <see cref="MeasurementVocabulary"/> — <em>how much?</em>
/// <see cref="PackagingVocabulary"/> — <em>how is it held?</em>
/// <see cref="SupplyVocabulary"/> — <em>how may it be supplied, and how
/// stored?</em> This one — <em>which groupings does this country belong
/// to?</em>
/// <para>
/// <b>No single authority publishes this list.</b> Every other vocabulary here
/// names a body that could one day replace it — EDQM, UCUM, ISO. Regions are
/// different: each grouping publishes only <em>its own</em> membership, and
/// nobody publishes the set of groupings. So this list is RegOS's own choice of
/// which groupings are worth recording, and it says so
/// (<see cref="CodingSystems.RegosInternal"/>, ADR-058 §6).
/// </para>
/// <para>
/// <b>They overlap, and that is why a country holds several.</b> Germany is EU
/// <em>and</em> ICH <em>and</em> PIC/S. The dead <c>RegionCode</c> column this
/// replaces could not have said that even if something had written to it.
/// </para>
/// </remarks>
public static class GeographyVocabulary
{
    /// <summary>
    /// The regulatory groupings a country may belong to.
    /// </summary>
    /// <remarks>
    /// Five, chosen because each one changes what a filing looks like: which
    /// procedure applies, which guidelines are adopted, whose inspection
    /// findings are recognised. A grouping that changes none of those is a fact
    /// about geography rather than about regulation, and does not belong here.
    /// </remarks>
    public static IReadOnlyList<CodedConcept> Regions { get; } =
    [
        CodedConcept.Internal("EU", "European Union"),
        CodedConcept.Internal("ICH", "ICH"),
        CodedConcept.Internal("PIC_S", "PIC/S"),
        CodedConcept.Internal("ASEAN", "ASEAN"),
        CodedConcept.Internal("GCC", "GCC"),
    ];

    public static CodedConcept? RegionOf(string? code)
        => CodedConceptLookup.Find(Regions, code);
}
