namespace RegOS.ReferenceData.Application.Queries.Substances.GetSubstanceVocabulary;

public sealed record SubstanceVocabularyDto(
    IReadOnlyList<CodedConceptDto> Classes,
    IReadOnlyList<CodedConceptDto> Types);
