using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.ReferenceData.Application.Queries.Clinical.GetClinicalVocabulary;

/// <summary>
/// Reads code, not the database — the vocabulary is versioned with the rule
/// that validates against it. It is still a query handler so that the day it is
/// read from a licensed terminology instead, only this file changes.
/// </summary>
public sealed class GetClinicalVocabularyHandler
{
    public Task<ClinicalVocabularyDto> HandleAsync(
        GetClinicalVocabularyQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ClinicalVocabularyDto(
            ClinicalConditionVocabulary.Conditions.Select(Dto).ToList(),
            ClinicalConditionVocabulary.Frequencies.Select(Dto).ToList(),
            ClinicalConditionVocabulary.PhysiologicalConditions
                .Select(Dto).ToList(),
            ClinicalConditionVocabulary.Genders.Select(Dto).ToList(),
            ClinicalConditionVocabulary.AgeUnits.Select(Dto).ToList(),
            ClinicalConditionVocabulary.TherapyRelationships
                .Select(Dto).ToList(),
            ClinicalConditionVocabulary.InteractionTypes.Select(Dto).ToList(),
            ClinicalConditionVocabulary.InteractionSeverities
                .Select(Dto).ToList()));
    }

    private static CodedConceptDto Dto(CodedConcept concept)
        => new(concept.System, concept.Code, concept.Display);
}
