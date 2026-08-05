namespace RegOS.ReferenceData.Application.Queries.Presentations.GetPharmaceuticalVocabulary;

/// <param name="UnitsOfPresentation">
/// Articles a patient is given — a vial, a tablet. <b>Not strength units.</b>
/// mg, mL and IU measure quantity and arrive with <c>Strength</c> in S003;
/// keeping the two apart is what stops one picker offering both.
/// </param>
/// <param name="ComponentTypes">
/// Sent alongside rather than from an endpoint of its own, unlike measurement
/// units. The component form needs dose forms too — a vial *of powder* — so one
/// payload is one fetch. Measurement units stayed apart because they are a
/// different axis: nothing here could be mistaken for a way to measure a
/// quantity, whereas "vial" beside "mL" invites exactly that.
/// </param>
/// <param name="Colours">
/// <b>Several may apply to one presentation</b> — a capsule with a white body
/// and a blue cap is two colours, not one called "white and blue".
/// </param>
/// <param name="Shapes">
/// Single-valued, unlike <paramref name="Colours"/>: a tablet is round or it is
/// oval, and nothing is both.
/// </param>
public sealed record PharmaceuticalVocabularyDto(
    IReadOnlyList<CodedConceptDto> DoseForms,
    IReadOnlyList<CodedConceptDto> RoutesOfAdministration,
    IReadOnlyList<CodedConceptDto> UnitsOfPresentation,
    IReadOnlyList<CodedConceptDto> ComponentTypes,
    IReadOnlyList<CodedConceptDto> Colours,
    IReadOnlyList<CodedConceptDto> Shapes);
