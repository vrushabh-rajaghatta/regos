using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using RegOS.Platform.Application.Services;
using RegOS.Platform.Infrastructure.Authentication;

namespace RegOS.Api.Authentication;

public static class AuthenticationRegistration
{
    /// <summary>
    /// Wires up bearer token validation.
    /// </summary>
    /// <remarks>
    /// Registered in the Host rather than in Platform.Infrastructure because
    /// the handler is an ASP.NET concern and Platform.Infrastructure is a plain
    /// library. The cost is that issuance and validation now live in two
    /// projects and must agree; they agree by both reading
    /// <see cref="JwtOptions"/>, so there is one place to change and no literal
    /// is written twice.
    /// </remarks>
    public static IServiceCollection AddRegOSAuthentication(
        this IServiceCollection services)
    {
        services.AddScoped<ICurrentUser, ClaimsCurrentUser>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // Bound from JwtOptions, which Platform.Infrastructure has already
        // validated at startup. A key that fails validation there stops the
        // application before this is ever read.
        services
            .AddOptions<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
            {
                var options = jwt.Value;

                // Off by default this would rewrite "sub" into a WS-Federation
                // URI and "email" likewise, so the claims read back under names
                // the issuer never wrote. Keeping it false means what
                // JwtAccessTokenIssuer puts in is exactly what ClaimsCurrentUser
                // takes out.
                bearer.MapInboundClaims = false;

                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    // Every one of these is on. A validation parameter turned
                    // off is a check that silently stops happening, and the
                    // defaults have changed between library versions before.
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = options.Issuer,
                    ValidAudience = options.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(options.SigningKey!)),

                    // The default allows five minutes of drift, which means a
                    // token stays usable for five minutes past its expiry. With
                    // a fifteen-minute lifetime that is a third of it.
                    ClockSkew = TimeSpan.Zero,

                    NameClaimType = "email"
                };
            });

        services.AddAuthorization();

        return services;
    }
}
