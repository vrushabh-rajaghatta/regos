namespace RegOS.RegulatoryApplication.Application.Queries.Applications.GetApplication;

/// <param name="ApplicationNumber">
/// Null until the authority has assigned one — the ordinary state of a new
/// application, not a missing value.
/// </param>
public sealed record ApplicationDetailDto(
    Guid Id,
    string Name,
    string? ApplicationNumber,
    Guid GlobalProductId,
    Guid CountryId,
    string CountryName,
    Guid AuthorityId,
    string AuthorityName,
    Guid ApplicantOrganizationId,
    string ApplicantOrganizationName,
    string Status,
    DateTime CreatedOn);
