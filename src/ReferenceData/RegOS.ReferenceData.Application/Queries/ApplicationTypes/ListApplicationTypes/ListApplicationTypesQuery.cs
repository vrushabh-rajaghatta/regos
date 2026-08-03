namespace RegOS.ReferenceData.Application.Queries.ApplicationTypes.ListApplicationTypes;

/// <param name="AuthorityId">
/// Optional. Narrows the list to one authority's application types — which is
/// how the application form uses it, because an application's type must belong
/// to its authority (see <c>RegulatoryApplication.Create</c>).
/// </param>
public sealed record ListApplicationTypesQuery(Guid? AuthorityId = null);
