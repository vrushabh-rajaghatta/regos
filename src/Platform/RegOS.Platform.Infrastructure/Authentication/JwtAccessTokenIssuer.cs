using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using RegOS.Platform.Application.Services;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Infrastructure.Authentication;

/// <summary>
/// Issues signed JWTs. Like <see cref="Services.PasswordHasher"/>, this writes
/// no cryptography of its own: the signing, encoding and format all belong to
/// the framework's token handler.
/// </summary>
public sealed class JwtAccessTokenIssuer : IAccessTokenIssuer
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _credentials;

    public JwtAccessTokenIssuer(IOptions<JwtOptions> options)
    {
        _options = options.Value;

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SigningKey!));

        _credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);
    }

    public AccessToken Issue(UserAggregate user)
    {
        // UtcNow once, so "issued at" and "expires at" cannot straddle a tick
        // and produce a token that looks issued after it expires.
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(_options.AccessTokenMinutes);

        var claims = new Dictionary<string, object>
        {
            // The user id is the subject; the tenant travels beside it and
            // is what every scoped query is filtered by. The role rides along
            // for the authorization policies — see RegOSClaims.Role for why
            // that stopped being forbidden (ADR-033). Still no name or status
            // claims: those really are snapshots nothing downstream checks.
            [JwtRegisteredClaimNames.Sub] = user.Id.Value.ToString(),
            [JwtRegisteredClaimNames.Email] = user.Email.Value,
            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            [RegOSClaims.Role] = user.Role.ToString()
        };

        // A platform user has no tenant, so their token carries no tenant
        // claim — absent, not empty. ITenantContext.TenantId throws for them
        // and the query filters resolve to no rows: platform identity never
        // silently widens into tenant access (ADR-030).
        if (user.TenantId is not null)
        {
            claims[RegOSClaims.TenantId] = user.TenantId.Value.ToString();
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = issuedAt,
            NotBefore = issuedAt,
            Expires = expiresAt,
            SigningCredentials = _credentials,
            Claims = claims
        };

        return new AccessToken(
            new JsonWebTokenHandler().CreateToken(descriptor),
            expiresAt);
    }
}
