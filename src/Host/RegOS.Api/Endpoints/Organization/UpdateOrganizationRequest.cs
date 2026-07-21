using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Api.Endpoints.Organization;

public sealed record UpdateOrganizationRequest(
    string? LegalName,
    OrganizationType Type);
