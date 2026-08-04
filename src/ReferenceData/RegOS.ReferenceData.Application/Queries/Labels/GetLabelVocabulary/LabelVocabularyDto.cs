namespace RegOS.ReferenceData.Application.Queries.Labels.GetLabelVocabulary;

public sealed record LabelVocabularyDto(
    IReadOnlyList<CodedConceptDto> GlobalLabelTypes);
