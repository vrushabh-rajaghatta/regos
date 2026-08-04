namespace RegOS.ReferenceData.Application.Queries.Clinical.GetClinicalVocabulary;

/// <param name="Conditions">
/// A deliberately tiny demonstration set. Nobody's real indication is in it —
/// see <c>ClinicalConditionVocabulary</c> for why that is stated rather than
/// discovered.
/// </param>
public sealed record ClinicalVocabularyDto(
    IReadOnlyList<CodedConceptDto> Conditions,
    IReadOnlyList<CodedConceptDto> PhysiologicalConditions,
    IReadOnlyList<CodedConceptDto> Genders,
    IReadOnlyList<CodedConceptDto> AgeUnits,
    IReadOnlyList<CodedConceptDto> TherapyRelationships);
