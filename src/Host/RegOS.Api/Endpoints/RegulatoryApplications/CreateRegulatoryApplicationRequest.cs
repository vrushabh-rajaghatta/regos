namespace RegOS.Api.Endpoints.RegulatoryApplications;

public sealed record CreateRegulatoryApplicationRequest(
    Guid CountryId,
    Guid AuthorityId,
    Guid ApplicationTypeId,
    Guid ApplicantOrganizationId,
    string Name);
