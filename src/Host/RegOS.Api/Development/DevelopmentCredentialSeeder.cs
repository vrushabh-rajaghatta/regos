using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Commands.SetUserPassword;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.Aggregates.UserCredential;
using RegOS.Platform.Domain.ValueObjects;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Api.Development;

/// <summary>
/// Creates one known sign-in account so the login flow can be exercised before
/// invitation acceptance exists.
///
/// Lives in the Host, not among the persistence seeders, for two reasons: it
/// needs the password hasher, which sits above persistence; and putting it here
/// keeps its <c>IsDevelopment</c> guard visible at the call site rather than
/// buried in a DI registration.
/// </summary>
public static class DevelopmentCredentialSeeder
{
    public const string EmailAddress = "dev@regos.local";

    // Not a secret. It exists only in Development, beside a signing key that is
    // equally non-secret, in a database seeded with fictional companies.
    public const string Password = "development-password";

    private static readonly Guid DemoManufacturerId =
        Guid.Parse("30000000-0000-0000-0000-000000000001");

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
            user = UserAggregate.Create(
                new OrganizationId(DemoManufacturerId),
                email,
                "Development",
                "User");

            // Created Invited, like every user. Sign-in requires Active, and
            // there is no invitation acceptance flow yet to do this properly.
            user.Activate();

            await users.AddAsync(user, CancellationToken.None);
        }

        var existing = await credentials.GetByUserIdAsync(
            user.Id, CancellationToken.None);

        // Only set a password when there is none, so a developer who changes it
        // locally does not have it reset on every restart.
        if (existing is not null) return;

        await setPassword.HandleAsync(
            new SetUserPasswordCommand(user.Id, Password),
            CancellationToken.None);
    }
}
