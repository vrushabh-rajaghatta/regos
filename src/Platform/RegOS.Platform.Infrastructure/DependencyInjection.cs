using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RegOS.Platform.Application.Services;
using RegOS.Platform.Infrastructure.Authentication;
using RegOS.Platform.Domain.Aggregates.RefreshToken;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.Aggregates.UserCredential;
using RegOS.Platform.Infrastructure.Repositories;
using RegOS.Platform.Infrastructure.Services;

namespace RegOS.Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<
            IUserCredentialRepository, UserCredentialRepository>();

        services.AddScoped<
            IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IUserPolicy, UserPolicy>();

        // Stateless and thread-safe, so a singleton. The framework hasher
        // allocates no per-request state.
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // Fails startup rather than the first login. A missing or too-short
        // signing key is a deployment mistake, and discovering it when someone
        // tries to sign in is discovering it too late. There is deliberately no
        // default and no generated fallback: the development path differs from
        // production only in where the secret comes from.
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SigningKey),
                $"{JwtOptions.SectionName}:SigningKey is required.")
            .Validate(
                options => string.IsNullOrWhiteSpace(options.SigningKey)
                    || Encoding.UTF8.GetByteCount(options.SigningKey)
                        >= JwtOptions.MinimumSigningKeyBytes,
                $"{JwtOptions.SectionName}:SigningKey must be at least "
                    + $"{JwtOptions.MinimumSigningKeyBytes} bytes.")
            .Validate(
                options => options.AccessTokenMinutes > 0,
                $"{JwtOptions.SectionName}:AccessTokenMinutes must be positive.")
            .ValidateOnStart();

        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();

        services.AddOptions<RefreshTokenOptions>()
            .Bind(configuration.GetSection(RefreshTokenOptions.SectionName))
            .Validate(
                options => options.Days > 0,
                $"{RefreshTokenOptions.SectionName}:Days must be positive.")
            .ValidateOnStart();

        services.AddSingleton<IRefreshTokenIssuer, RefreshTokenIssuer>();

        return services;
    }
}
