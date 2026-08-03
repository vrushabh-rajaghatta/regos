using RegOS.ReferenceData.Domain.Substances;
using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.Persistence.Initialization.ReferenceData;

/// <summary>
/// Six well-known compounds, so the directory is not empty on first login.
/// </summary>
/// <remarks>
/// <b>Demonstration seed data only. These records intentionally do not
/// represent the authoritative GSRS/UNII or ISO 11238 substance registry.
/// Licensed and authoritative terminology is introduced separately.</b>
/// <para>
/// Every row carries <c>System = "regos-internal"</c> and <b>no external
/// identifier at all</b> — no UNII, no CAS, no ATC. That is not an unfinished
/// seed: a populated <c>UniiCode</c> here would claim RegOS holds GSRS, which
/// it does not. A null one is a fact about what the platform has, and the
/// distinction is the whole reason ADR-058 §6 exists.
/// </para>
/// <para>
/// <b>This list is not trying to be complete and never will be.</b> It is six
/// molecules a reviewer will recognise. The catalogue becomes real when
/// licensed terminology is obtained, and until then a tenant's own compounds
/// sit beside these on equal footing.
/// </para>
/// <para>
/// <c>Aspirin</c> is the row worth reading: its preferred name and its INN
/// genuinely differ, which is why <c>Name</c> and <c>Inn</c> are two fields
/// rather than one (EPIC-010a D7). The other five agree, and a seed where they
/// all agreed would make the second field look redundant.
/// </para>
/// </remarks>
internal static class Substances
{
    // Properties, not `static readonly` fields — each access builds a new
    // instance. A CodedConcept is persisted as an owned entity, and EF tracks
    // one against exactly one owner; handing the same object to six substances
    // makes five of them look like they have no class at all.
    private static CodedConcept Chemical =>
        CodedConcept.Internal("CHEMICAL", "Chemical");

    private static CodedConcept Synthetic =>
        CodedConcept.Internal("SYNTHETIC", "Synthetic");

    private static CodedConcept SemiSynthetic =>
        CodedConcept.Internal("SEMI_SYNTHETIC", "Semi-synthetic");

    public static IReadOnlyList<Substance> Data =>
    [
        Substance.Seed(
            new SubstanceId(SubstanceIds.Paracetamol),
            name: "Paracetamol",
            inn: "Paracetamol",
            substanceClass: Chemical,
            substanceType: Synthetic,
            molecularFormula: "C8H9NO2",
            description: "Analgesic and antipyretic."),

        Substance.Seed(
            new SubstanceId(SubstanceIds.Ibuprofen),
            name: "Ibuprofen",
            inn: "Ibuprofen",
            substanceClass: Chemical,
            substanceType: Synthetic,
            molecularFormula: "C13H18O2",
            description: "Non-steroidal anti-inflammatory drug."),

        Substance.Seed(
            new SubstanceId(SubstanceIds.Amoxicillin),
            name: "Amoxicillin",
            inn: "Amoxicillin",
            substanceClass: Chemical,
            // Semi-synthetic, because it is derived from a fermentation
            // product. The one seeded row where the type is not the default,
            // so that the field is visibly doing work.
            substanceType: SemiSynthetic,
            molecularFormula: "C16H19N3O5S",
            description: "Beta-lactam antibacterial."),

        Substance.Seed(
            new SubstanceId(SubstanceIds.Metformin),
            name: "Metformin",
            inn: "Metformin",
            substanceClass: Chemical,
            substanceType: Synthetic,
            molecularFormula: "C4H11N5",
            description: "Biguanide antihyperglycaemic."),

        Substance.Seed(
            new SubstanceId(SubstanceIds.Aspirin),
            name: "Aspirin",
            inn: "Acetylsalicylic acid",
            substanceClass: Chemical,
            substanceType: Synthetic,
            molecularFormula: "C9H8O4",
            description: "Salicylate analgesic and antiplatelet agent."),

        Substance.Seed(
            new SubstanceId(SubstanceIds.Omeprazole),
            name: "Omeprazole",
            inn: "Omeprazole",
            substanceClass: Chemical,
            substanceType: Synthetic,
            molecularFormula: "C17H19N3O3S",
            description: "Proton pump inhibitor.")
    ];
}
