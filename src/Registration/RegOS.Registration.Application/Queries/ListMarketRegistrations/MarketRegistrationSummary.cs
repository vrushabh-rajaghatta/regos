namespace RegOS.Registration.Application.Queries.ListMarketRegistrations;

/// <summary>
/// A row in "what do we hold in this market?" — keyed by product, because the
/// country is the question rather than an answer.
/// </summary>
/// <remarks>
/// Deliberately <em>not</em> the same record as
/// <c>ListProductRegistrations.RegistrationSummary</c>. The two are mirror
/// images: one repeats the product in every row, the other the country. A single
/// DTO carrying both would leave every consumer ignoring half its fields, which
/// is coupling rather than reuse.
/// </remarks>
public sealed record MarketRegistrationSummary(
    Guid RegistrationId,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    Guid AuthorityId,
    string AuthorityName,
    string HolderOrganizationName,
    string? RegistrationNumber,
    string Status,
    DateOnly? ApprovedOn,
    DateOnly? ExpiresOn);
