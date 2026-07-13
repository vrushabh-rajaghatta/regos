namespace RegOS.Api.Endpoints.RegulatoryApplications;

public sealed record CreateRegulatoryApplicationRequest(
    Guid AuthorityId,
    Guid CountryId,
    Guid ApplicantOrganizationId,
    string Name);
