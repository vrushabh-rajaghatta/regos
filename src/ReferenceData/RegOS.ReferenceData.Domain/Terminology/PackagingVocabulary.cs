namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// The words a pack's layers may be drawn from — what each layer <em>is</em>,
/// and what it is <em>made of</em>.
/// </summary>
/// <remarks>
/// <b>Its own list, deliberately not folded into
/// <see cref="PharmaceuticalVocabulary"/>.</b> That one answers <em>what is this
/// medicine?</em> — dose form, route, unit of presentation. This answers
/// <em>how is it held?</em>, and offering "blister" beside "tablet" in one
/// payload would be the first step towards a pack stating what the presentation
/// already says. The same call <see cref="MeasurementVocabulary"/> made about
/// strength units.
/// <para>
/// <b>Material is the attribute that makes a package item not a component</b>
/// (<see href="../../../docs/adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md">ADR-061</see>
/// §1): a component has a dose form, a package item has a material.
/// </para>
/// <para>
/// <b>EDQM's concepts, not EDQM's terms.</b> Container type and material are
/// both EDQM Standard Terms in the real world; RegOS holds no such licence, so
/// every entry is <see cref="CodingSystems.RegosInternal"/> and says so, with
/// codes deliberately unlike EDQM's so one can never be mistaken for the other
/// (ADR-058 §6).
/// </para>
/// </remarks>
public static class PackagingVocabulary
{
    /// <summary>What a layer of the pack is.</summary>
    /// <remarks>
    /// The outermost is usually a carton; the innermost holds the product
    /// itself. A pallet and a shipper are here because a supply chain has them,
    /// not because anyone has asked to record one yet.
    /// </remarks>
    public static IReadOnlyList<CodedConcept> PackageItemTypes { get; } =
    [
        CodedConcept.Internal("CARTON", "Carton"),
        CodedConcept.Internal("BLISTER", "Blister"),
        CodedConcept.Internal("WALLET", "Wallet"),
        CodedConcept.Internal("BOTTLE", "Bottle"),
        CodedConcept.Internal("VIAL", "Vial"),
        CodedConcept.Internal("AMPOULE", "Ampoule"),
        CodedConcept.Internal("PRE_FILLED_SYRINGE", "Pre-filled syringe"),
        CodedConcept.Internal("SACHET", "Sachet"),
        CodedConcept.Internal("TUBE", "Tube"),
        CodedConcept.Internal("SHIPPER", "Shipping container"),
    ];

    /// <summary>What that layer is made of.</summary>
    /// <remarks>
    /// Optional on a package item: an outer carton's board grade is rarely
    /// stated, while a blister's laminate always is — it is what the stability
    /// data was generated against.
    /// </remarks>
    public static IReadOnlyList<CodedConcept> Materials { get; } =
    [
        CodedConcept.Internal("PVC_ALU", "PVC/aluminium"),
        CodedConcept.Internal("PVDC_ALU", "PVC/PVdC/aluminium"),
        CodedConcept.Internal("ALU_ALU", "Aluminium/aluminium"),
        CodedConcept.Internal("HDPE", "High-density polyethylene"),
        CodedConcept.Internal("PP", "Polypropylene"),
        CodedConcept.Internal("GLASS_TYPE_I", "Type I glass"),
        CodedConcept.Internal("GLASS_TYPE_III", "Type III glass"),
        CodedConcept.Internal("PAPERBOARD", "Paperboard"),
        CodedConcept.Internal("LAMINATED_FOIL", "Laminated foil"),
    ];

    public static CodedConcept? PackageItemTypeOf(string? code)
        => CodedConceptLookup.Find(PackageItemTypes, code);

    public static CodedConcept? MaterialOf(string? code)
        => CodedConceptLookup.Find(Materials, code);
}
