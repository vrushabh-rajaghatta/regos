using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Organization.Application.Queries.Divisions.ListOrganizationDivisions;

/// <summary>"Which business units does this organization have?"</summary>
public sealed record ListOrganizationDivisionsQuery(OrganizationId OrganizationId);
