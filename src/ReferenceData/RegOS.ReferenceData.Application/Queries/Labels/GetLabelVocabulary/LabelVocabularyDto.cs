namespace RegOS.ReferenceData.Application.Queries.Labels.GetLabelVocabulary;

/// <param name="LocalLabelTypes">
/// Carton artwork is in this list, not in one of its own — a printed carton is
/// a controlled document an authority approved, revised exactly as a leaflet is
/// (EPIC-018 D2).
/// </param>
public sealed record LabelVocabularyDto(
    IReadOnlyList<CodedConceptDto> GlobalLabelTypes,
    IReadOnlyList<CodedConceptDto> LocalLabelTypes);
