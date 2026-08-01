namespace RegOS.ReferenceData.Application.Queries.Regulatory.ListAuthorityDivisions;

/// <param name="IsTenantDefined">
/// True when this tenant added it, false when the platform ships it. Surfaced
/// so a user can tell their own local knowledge from a governed fact.
/// </param>
public sealed record AuthorityDivisionDto(
    Guid Id,
    Guid AuthorityId,
    string Name,
    bool IsTenantDefined);
