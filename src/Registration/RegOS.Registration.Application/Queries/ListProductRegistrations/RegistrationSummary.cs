namespace RegOS.Registration.Application.Queries.ListProductRegistrations;

/// <summary>
/// A row in "where is this product registered?" — the market, the status, the
/// number, and the dates that govern it.
/// </summary>
public sealed record RegistrationSummary(
    Guid RegistrationId,
    Guid CountryId,
    string CountryName,
    Guid AuthorityId,
    string AuthorityName,
    string HolderOrganizationName,
    string? RegistrationNumber,
    string Status,
    DateOnly? ApprovedOn,
    DateOnly? ExpiresOn);
