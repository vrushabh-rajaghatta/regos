namespace RegOS.RegulatoryApplication.Application.Queries.Applications.GetApplication;

public sealed record ApplicationDetailDto(
    Guid Id,
    string Name,
    Guid ProductId,
    Guid CountryId,
    string CountryName,
    Guid AuthorityId,
    string AuthorityName,
    Guid ApplicantOrganizationId,
    string ApplicantOrganizationName,
    string Status,
    DateTime CreatedOn);
