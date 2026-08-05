using RegOS.ReferenceData.Application.Queries.Presentations.GetPharmaceuticalVocabulary;
using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.ReferenceData.Application.Queries.Supply.GetSupplyVocabulary;

/// <summary>
/// Reads code, not the database — the vocabulary is versioned with the rule that
/// validates against it. Still a query handler so that the day it is read from a
/// licensed dataset instead, only this file changes.
/// </summary>
public sealed class GetSupplyVocabularyHandler
{
    public Task<SupplyVocabularyDto> HandleAsync(
        GetSupplyVocabularyQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SupplyVocabularyDto(
            SupplyVocabulary.LegalStatuses.Select(Dto).ToList(),
            SupplyVocabulary.StorageConditions.Select(Dto).ToList(),
            SupplyVocabulary.ShelfLifePeriods.Select(Dto).ToList(),

            // A different class, deliberately — Geography reads this same list
            // to say what a market accepts, so neither vocabulary owns it.
            StabilityVocabulary.Conditions.Select(Dto).ToList()));
    }

    private static CodedConceptDto Dto(CodedConcept concept)
        => new(concept.System, concept.Code, concept.Display);
}
