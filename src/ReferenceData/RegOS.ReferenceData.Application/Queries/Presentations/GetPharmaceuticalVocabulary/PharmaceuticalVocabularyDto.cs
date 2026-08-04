namespace RegOS.ReferenceData.Application.Queries.Presentations.GetPharmaceuticalVocabulary;

/// <param name="UnitsOfPresentation">
/// Articles a patient is given — a vial, a tablet. <b>Not strength units.</b>
/// mg, mL and IU measure quantity and arrive with <c>Strength</c> in S003;
/// keeping the two apart is what stops one picker offering both.
/// </param>
public sealed record PharmaceuticalVocabularyDto(
    IReadOnlyList<PharmaceuticalConceptDto> DoseForms,
    IReadOnlyList<PharmaceuticalConceptDto> RoutesOfAdministration,
    IReadOnlyList<PharmaceuticalConceptDto> UnitsOfPresentation);

public sealed record PharmaceuticalConceptDto(
    string System,
    string Code,
    string Display);
