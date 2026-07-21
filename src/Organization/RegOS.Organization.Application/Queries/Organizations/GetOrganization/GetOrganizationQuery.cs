using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Organization.Application.Queries.Organizations.GetOrganization;

public sealed record GetOrganizationQuery(OrganizationId Id);
