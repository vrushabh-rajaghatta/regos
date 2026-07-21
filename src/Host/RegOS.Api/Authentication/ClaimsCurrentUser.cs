using System.Security.Claims;

using Microsoft.IdentityModel.JsonWebTokens;

using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.Platform.Infrastructure.Authentication;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Api.Authentication;

/// <summary>
/// Reads the current user from the validated token's claims.
/// </summary>
/// <remarks>
/// <para>
/// Lives in the Host, like <see cref="Tenancy.ClaimsTenantContext"/>, because
/// it is an adapter from HTTP to an abstraction the application layer defines.
/// Platform.Application knows there is a current user; only the Host knows one
/// arrives as a bearer token.
/// </para>
/// <para>
/// Nothing here checks a signature or an expiry. By the time this runs the
/// authentication handler has already rejected anything unsigned, expired, or
/// issued by someone else — so a present claim is a trustworthy claim, and a
/// malformed one is a bug in the issuer rather than an attack.
/// </para>
/// </remarks>
public sealed class ClaimsCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimsCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated ?? false;

    public UserId UserId =>
        new(RequiredGuid(JwtRegisteredClaimNames.Sub));

    public TenantId TenantId =>
        new(RequiredGuid(RegOSClaims.TenantId));

    public Email Email =>
        Email.Create(Required(JwtRegisteredClaimNames.Email));

    private string Required(string claimType)
    {
        // Anonymous first, so an endpoint that forgot RequireAuthorization
        // fails with "who are you" rather than with a claim-shaped complaint
        // that reads like the token was broken.
        if (!IsAuthenticated)
        {
            throw new AuthenticationFailedException(
                CurrentUserErrors.NotAuthenticated);
        }

        var value = Principal!.FindFirstValue(claimType);

        return string.IsNullOrWhiteSpace(value)
            ? throw new AuthenticationFailedException(
                CurrentUserErrors.NotAuthenticated)
            : value;
    }

    private Guid RequiredGuid(string claimType) =>
        Guid.TryParse(Required(claimType), out var value)
            && value != Guid.Empty
                ? value
                : throw new AuthenticationFailedException(
                    CurrentUserErrors.NotAuthenticated);
}
