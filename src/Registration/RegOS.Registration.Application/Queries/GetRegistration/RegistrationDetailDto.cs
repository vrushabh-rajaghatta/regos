namespace RegOS.Registration.Application.Queries.GetRegistration;

/// <param name="RegistrationNumber">
/// The authority's number — the registration's business identity. Null until
/// the grant is recorded.
/// </param>
/// <param name="History">
/// Every status held, oldest first. Append-only, so this is the whole record of
/// how the registration reached its current state.
/// </param>
public sealed record RegistrationDetailDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid CountryId,
    string CountryName,
    Guid AuthorityId,
    string AuthorityName,
    Guid HolderOrganizationId,
    string HolderOrganizationName,
    Guid? OriginatingApplicationId,
    string? RegistrationNumber,
    string Status,
    DateOnly? ApprovedOn,
    DateOnly? ExpiresOn,
    DateTime CreatedOn,
    IReadOnlyList<RegistrationStatusEntryDto> History);

/// <param name="OccurredOn">When it happened in the world.</param>
/// <param name="RecordedOnUtc">When RegOS learned of it.</param>
public sealed record RegistrationStatusEntryDto(
    Guid Id,
    string Status,
    DateOnly OccurredOn,
    DateTime RecordedOnUtc,
    string? Note);
