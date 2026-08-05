using RegOS.ReferenceData.Application.Queries.Presentations.GetPharmaceuticalVocabulary;
using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.ReferenceData.Application.Queries.Packaging.GetPackagingVocabulary;

/// <summary>
/// Reads code, not the database — the vocabulary is versioned with the rule that
/// validates against it. Still a query handler so that the day it is read from a
/// licensed dataset instead, only this file changes.
/// </summary>
public sealed class GetPackagingVocabularyHandler
{
    public Task<PackagingVocabularyDto> HandleAsync(
        GetPackagingVocabularyQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PackagingVocabularyDto(
            PackagingVocabulary.PackageItemTypes.Select(Dto).ToList(),
            PackagingVocabulary.Materials.Select(Dto).ToList()));
    }

    private static CodedConceptDto Dto(CodedConcept concept)
        => new(concept.System, concept.Code, concept.Display);
}
