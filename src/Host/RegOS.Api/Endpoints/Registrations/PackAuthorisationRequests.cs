namespace RegOS.Api.Endpoints.Registrations;

/// <param name="AuthorisedOn">
/// Supplied, never read from the clock: a licence granted in 2021 that gained
/// its 100-pack in 2024 by variation has two dates, and only one of them is the
/// registration's.
/// </param>
public sealed record AuthorisePackRequest(
    Guid PackagedProductId,
    DateOnly AuthorisedOn);

public sealed record PackAuthorisationResponse(Guid Id);
