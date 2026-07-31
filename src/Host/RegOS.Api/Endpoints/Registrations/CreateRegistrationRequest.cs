namespace RegOS.Api.Endpoints.Registrations;

/// <param name="OccurredOn">
/// The business date this registration entered its first status — supplied, not
/// taken from the clock, so a migrated portfolio can state when things happened.
/// </param>
/// <param name="OriginatingApplicationId">
/// Optional: acquired and in-licensed products carry authorisations RegOS never
/// witnessed being filed.
/// </param>
public sealed record CreateRegistrationRequest(
    Guid CountryId,
    Guid AuthorityId,
    Guid HolderOrganizationId,
    DateOnly OccurredOn,
    Guid? OriginatingApplicationId = null,
    string? Note = null);
