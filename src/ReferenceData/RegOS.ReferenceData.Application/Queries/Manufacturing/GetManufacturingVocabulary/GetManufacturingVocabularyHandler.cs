using RegOS.ReferenceData.Application.Queries.Presentations.GetPharmaceuticalVocabulary;
using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.ReferenceData.Application.Queries.Manufacturing.GetManufacturingVocabulary;

/// <summary>
/// Reads code, not the database — the vocabulary is versioned with the rule that
/// validates against it. Still a query handler so that the day it is read from a
/// licensed dataset instead, only this file changes.
/// </summary>
public sealed class GetManufacturingVocabularyHandler
{
    public Task<ManufacturingVocabularyDto> HandleAsync(
        GetManufacturingVocabularyQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ManufacturingVocabularyDto(
            ManufacturingVocabulary.Operations.Select(Dto).ToList()));
    }

    private static CodedConceptDto Dto(CodedConcept concept)
        => new(concept.System, concept.Code, concept.Display);
}
