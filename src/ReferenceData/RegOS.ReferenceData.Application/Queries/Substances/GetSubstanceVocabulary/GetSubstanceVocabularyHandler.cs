using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.ReferenceData.Application.Queries.Substances.GetSubstanceVocabulary;

/// <summary>
/// Reads code, not the database — the vocabulary is versioned with the rule
/// that validates against it (<see cref="SubstanceVocabulary"/>). It is still a
/// query handler so that the day it is read from a licensed dataset instead,
/// only this file changes.
/// </summary>
public sealed class GetSubstanceVocabularyHandler
{
    public Task<SubstanceVocabularyDto> HandleAsync(
        GetSubstanceVocabularyQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SubstanceVocabularyDto(
            SubstanceVocabulary.Classes.Select(Dto).ToList(),
            SubstanceVocabulary.Types.Select(Dto).ToList()));
    }

    private static CodedConceptDto Dto(CodedConcept concept)
        => new(concept.System, concept.Code, concept.Display);
}
