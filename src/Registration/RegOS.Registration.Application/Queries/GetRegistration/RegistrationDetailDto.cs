namespace RegOS.Registration.Application.Queries.GetRegistration;

/// <param name="RegistrationNumber">
/// The authority's number — the registration's business identity. Null until
/// the grant is recorded.
/// </param>
/// <param name="History">
/// Every status held, oldest first. Append-only, so this is the whole record of
/// how the registration reached its current state.
/// </param>
/// <param name="AllowedNextStatuses">
/// Where this registration may go from here, straight from the domain's
/// transition table — so a client offers exactly the choices the domain would
/// accept instead of restating the rules. Empty when the status is terminal.
/// <para>
/// Reaching <c>Approved</c> for the first time is the one entry that is not a
/// plain status change: it goes through the approval endpoint, which captures
/// the registration number and validity dates. Returning to <c>Approved</c> from
/// <c>Suspended</c> is an ordinary transition.
/// </para>
/// </param>
/// <param name="DaysUntilExpiry">
/// Derived on every read, never stored — see
/// <see cref="RegOS.Registration.Application.Queries.ExpiryVisibility"/>.
/// </param>
public sealed record RegistrationDetailDto(
    Guid Id,
    Guid GlobalProductId,
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
    IReadOnlyList<RegistrationStatusEntryDto> History,
    IReadOnlyList<string> AllowedNextStatuses,
    bool HasRunningValidity,
    int? DaysUntilExpiry,
    bool IsExpired);

/// <param name="OccurredOn">When it happened in the world.</param>
/// <param name="RecordedOnUtc">When RegOS learned of it.</param>
public sealed record RegistrationStatusEntryDto(
    Guid Id,
    string Status,
    DateOnly OccurredOn,
    DateTime RecordedOnUtc,
    string? Note);
