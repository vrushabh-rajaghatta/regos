using RegOS.ReferenceData.Domain.Regulatory.Authority;

namespace RegOS.ReferenceData.Application.Queries.Regulatory.ListAuthorityDivisions;

/// <summary>
/// The divisions a caller may name for one authority — the platform's plus
/// their own. Scoped to the authority because the picker is only ever opened
/// after one has been chosen, and an unscoped list would offer a Health Canada
/// bureau on an FDA letter.
/// </summary>
public sealed record ListAuthorityDivisionsQuery(AuthorityId AuthorityId);
