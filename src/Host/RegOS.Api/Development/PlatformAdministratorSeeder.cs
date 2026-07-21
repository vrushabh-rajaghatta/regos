using RegOS.Platform.Application.Commands.SetUserPassword;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.Aggregates.UserCredential;
using RegOS.Platform.Domain.ValueObjects;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Api.Development;

/// <summary>
/// Creates one platform user — a person who operates RegOS itself and belongs
/// to no tenant (ADR-030) — so the null-tenant path can be exercised end to
/// end: sign-in, a token with no tenant claim, and every tenant-scoped query
/// resolving to nothing.
/// </summary>
/// <remarks>
/// Development only, same guard and same shape as
/// <see cref="DevelopmentCredentialSeeder"/>. What this account deliberately
/// does NOT have is any cross-tenant power: no claim, no policy and no
/// endpoint grants it one yet. That is the authorization slice's job, with its
/// own ADR — this seeder only proves the account shape works.
/// </remarks>
public static class PlatformAdministratorSeeder
{
    public const string EmailAddress = "platform@regos.local";

    // Not a secret, for the same reason as the development credential: it
    // exists only in Development, beside a signing key that is equally
    // non-secret.
    public const string Password = "platform-password";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var users = services.GetRequiredService<IUserRepository>();
        var credentials =
            services.GetRequiredService<IUserCredentialRepository>();
        var setPassword =
            services.GetRequiredService<SetUserPasswordHandler>();

        var email = Email.Create(EmailAddress);

        var user = await users.GetByEmailAsync(email, CancellationToken.None);

        if (user is null)
        {
            user = UserAggregate.CreatePlatformUser(
                email,
                "Platform",
                "Administrator");

            // Created Invited, like every user. Sign-in requires Active, and
            // a platform user has no tenant administrator to invite them.
            user.Activate();

            await users.AddAsync(user, CancellationToken.None);
        }

        var existing = await credentials.GetByUserIdAsync(
            user.Id, CancellationToken.None);

        // Only set a password when there is none, so a developer who changes
        // it locally does not have it reset on every restart.
        if (existing is not null) return;

        await setPassword.HandleAsync(
            new SetUserPasswordCommand(user.Id, Password),
            CancellationToken.None);
    }
}
