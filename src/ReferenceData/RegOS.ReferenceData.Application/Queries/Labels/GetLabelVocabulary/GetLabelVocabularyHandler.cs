using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.ReferenceData.Application.Queries.Labels.GetLabelVocabulary;

/// <summary>
/// Reads code, not the database — the vocabulary is versioned with the rule that
/// validates against it (<see cref="LabelVocabulary"/>). It is still a query
/// handler so that the day it is read from a licensed dataset instead, only this
/// file changes.
/// </summary>
public sealed class GetLabelVocabularyHandler
{
    public Task<LabelVocabularyDto> HandleAsync(
        GetLabelVocabularyQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new LabelVocabularyDto(
            LabelVocabulary.GlobalLabelTypes
                .Select(x => new CodedConceptDto(x.System, x.Code, x.Display))
                .ToList()));
    }
}
