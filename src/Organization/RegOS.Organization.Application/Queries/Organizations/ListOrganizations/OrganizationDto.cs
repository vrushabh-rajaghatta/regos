using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Organization.Application.Queries.Organizations.ListOrganizations;

public sealed record OrganizationDto(
    Guid Id,
    string LegalName,
    OrganizationType Type,
    OrganizationStatus Status);
