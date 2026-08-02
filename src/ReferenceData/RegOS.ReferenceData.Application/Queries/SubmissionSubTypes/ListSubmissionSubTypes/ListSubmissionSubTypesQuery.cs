namespace RegOS.ReferenceData.Application.Queries.SubmissionSubTypes.ListSubmissionSubTypes;

/// <param name="AuthorityId">
/// Optional. Narrows the list to one authority's vocabulary — which is how the
/// submission form uses it, because a sequence's classification must belong to
/// the authority its application is filed with.
/// </param>
public sealed record ListSubmissionSubTypesQuery(Guid? AuthorityId = null);
