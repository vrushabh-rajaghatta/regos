namespace RegOS.ReferenceData.Application.Queries.SubmissionTypes.ListSubmissionTypes;

/// <param name="AuthorityId">
/// Optional. Narrows the list to one authority's vocabulary — which is how the
/// submission form uses it, because a sequence's classification must belong to
/// the authority its application is filed with.
/// </param>
public sealed record ListSubmissionTypesQuery(Guid? AuthorityId = null);
