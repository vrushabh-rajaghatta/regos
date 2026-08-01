namespace RegOS.Registration.Application.Queries.ListProductRegistrations;

/// <summary>
/// A row in "where is this product registered?" — the market, the status, the
/// number, and the dates that govern it.
/// </summary>
/// <param name="DaysUntilExpiry">
/// Derived on read, never stored. Null when there is no expiry date or the
/// lifecycle has ended; negative once the date has passed.
/// </param>
public sealed record RegistrationSummary(
    Guid RegistrationId,
    Guid MedicinalProductId,
    Guid CountryId,
    string CountryName,
    Guid AuthorityId,
    string AuthorityName,
    string HolderOrganizationName,
    string? RegistrationNumber,
    string Status,
    DateOnly? ApprovedOn,
    DateOnly? ExpiresOn,
    bool HasRunningValidity,
    int? DaysUntilExpiry,
    bool IsExpired);
