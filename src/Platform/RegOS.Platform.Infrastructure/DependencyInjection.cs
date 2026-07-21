using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RegOS.Platform.Application.Services;
using RegOS.Platform.Infrastructure.Authentication;
using RegOS.Platform.Domain.Aggregates.Invitation;
using RegOS.Platform.Domain.Aggregates.PasswordReset;
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

        services.AddScoped<IInvitationRepository, InvitationRepository>();

        services.AddScoped<
            IPasswordResetRepository, PasswordResetRepository>();

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

        // Shared by both token issuers: generation and hashing are the same
        // problem for a refresh token and an invitation; only the lifetime
        // differs, and that is what each issuer owns (ADR-027).
        services.AddSingleton<SecretTokenFactory>();

        services.AddSingleton<IRefreshTokenIssuer, RefreshTokenIssuer>();

        services.AddOptions<InvitationOptions>()
            .Bind(configuration.GetSection(InvitationOptions.SectionName))
            .Validate(
                options => options.Days > 0,
                $"{InvitationOptions.SectionName}:Days must be positive.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.AcceptUrl),
                $"{InvitationOptions.SectionName}:AcceptUrl is required.")
            .ValidateOnStart();

        services.AddSingleton<IInvitationTokenIssuer, InvitationTokenIssuer>();

        // The default, and the only one outside Development. It records that
        // delivery did not happen and deliberately never logs the token.
        services.AddScoped<
            IInvitationNotifier, UnconfiguredInvitationNotifier>();

        services.AddOptions<PasswordResetOptions>()
            .Bind(configuration.GetSection(PasswordResetOptions.SectionName))
            .Validate(
                options => options.Minutes > 0,
                $"{PasswordResetOptions.SectionName}:Minutes must be positive.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.CompleteUrl),
                $"{PasswordResetOptions.SectionName}:CompleteUrl is required.")
            .ValidateOnStart();

        services.AddSingleton<
            IPasswordResetTokenIssuer, PasswordResetTokenIssuer>();

        services.AddScoped<
            IPasswordResetNotifier, UnconfiguredPasswordResetNotifier>();

        return services;
    }
}
