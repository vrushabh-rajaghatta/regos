namespace RegOS.Registration.Application.Queries.ListExpiringRegistrations;

/// <summary>
/// A registration whose validity period is still running, and when it ends.
/// </summary>
/// <remarks>
/// Carries both axes, unlike the two portfolio summaries: this list spans the
/// whole book, so neither the product nor the market is implied by where you
/// are standing.
/// </remarks>
/// <param name="DaysUntilExpiry">
/// Never null here — a registration only appears in this list because it has an
/// expiry date and is still on the timeline. Negative once the date has passed.
/// </param>
public sealed record ExpiringRegistration(
    Guid RegistrationId,
    Guid ProductId,
    string ProductName,
    Guid CountryId,
    string CountryName,
    string? RegistrationNumber,
    string Status,
    DateOnly ExpiresOn,
    int DaysUntilExpiry,
    bool IsExpired);
