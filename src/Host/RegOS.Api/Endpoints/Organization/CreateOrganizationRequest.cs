using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Api.Endpoints.Organization;

public sealed record CreateOrganizationRequest(
    string? LegalName,
    OrganizationType Type);
