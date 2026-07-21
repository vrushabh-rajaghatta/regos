using RegOS.Platform.Domain.Aggregates.User;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Services;

/// <summary>
/// Issues the bearer token that proves a user signed in. The interface exists so
/// the application layer never names a token library or a signing algorithm,
/// exactly as <see cref="IPasswordHasher"/> hides the hashing implementation.
/// </summary>
public interface IAccessTokenIssuer
{
    AccessToken Issue(UserAggregate user);
}

/// <param name="Value">The encoded token.</param>
/// <param name="ExpiresAt">
/// When the token stops being accepted. Returned to the client so it can
/// refresh before expiry rather than discovering it through a failed request.
/// </param>
public sealed record AccessToken(string Value, DateTime ExpiresAt);
