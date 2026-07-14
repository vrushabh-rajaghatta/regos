namespace RegOS.Api.Endpoints.RegulatoryApplications;

public sealed record CreateRegulatoryApplicationRequest(
    Guid CountryId,
    Guid AuthorityId,
    Guid ApplicantOrganizationId,
    string Name);
