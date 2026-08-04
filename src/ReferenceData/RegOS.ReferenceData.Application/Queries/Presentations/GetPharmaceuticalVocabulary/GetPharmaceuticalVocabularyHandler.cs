using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.ReferenceData.Application.Queries.Presentations.GetPharmaceuticalVocabulary;

/// <summary>
/// Reads code, not the database — the vocabulary is versioned with the rule
/// that validates against it. It is still a query handler so that the day it is
/// read from a licensed dataset instead, only this file changes.
/// </summary>
public sealed class GetPharmaceuticalVocabularyHandler
{
    public Task<PharmaceuticalVocabularyDto> HandleAsync(
        GetPharmaceuticalVocabularyQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PharmaceuticalVocabularyDto(
            PharmaceuticalVocabulary.DoseForms.Select(Dto).ToList(),
            PharmaceuticalVocabulary.RoutesOfAdministration.Select(Dto).ToList(),
            PharmaceuticalVocabulary.UnitsOfPresentation.Select(Dto).ToList(),
            PharmaceuticalVocabulary.ComponentTypes.Select(Dto).ToList()));
    }

    private static CodedConceptDto Dto(CodedConcept concept)
        => new(concept.System, concept.Code, concept.Display);
}
