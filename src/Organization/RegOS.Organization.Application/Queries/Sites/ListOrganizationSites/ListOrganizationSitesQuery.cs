using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Organization.Application.Queries.Sites.ListOrganizationSites;

/// <summary>"Which sites does this organization operate?"</summary>
public sealed record ListOrganizationSitesQuery(OrganizationId OrganizationId);
