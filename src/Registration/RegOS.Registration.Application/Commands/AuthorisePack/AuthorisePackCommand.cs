using RegOS.Product.Domain.Product;
using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Registration.Application.Commands.AuthorisePack;

/// <summary>
/// Records that a licence authorises a pack, from a date.
/// </summary>
/// <param name="AuthorisedOn">
/// <b>Supplied, never read from the clock.</b> A licence granted in 2021 that
/// gained its 100-pack in 2024 by variation has two dates, and only one of them
/// is the registration's — which is the whole reason this is a relationship
/// rather than a foreign key (ADR-061 §3).
/// </param>
public sealed record AuthorisePackCommand(
    RegistrationId RegistrationId,
    PackagedProductId PackagedProductId,
    DateOnly AuthorisedOn);
