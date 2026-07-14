namespace RegOS.RegulatoryApplication.Application.Queries.GetRegulatoryApplication;

public sealed record RegulatoryApplicationDetail(
    Guid Id,
    string Name,
    string? ApplicationNumber,
    string Status,
    string CountryName,
    string CountryCode,
    string AuthorityName,
    string AuthorityCode,
    string OrganizationName);
