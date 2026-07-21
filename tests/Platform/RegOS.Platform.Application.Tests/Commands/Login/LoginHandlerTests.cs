using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.Platform.Application.Commands.Login;
using RegOS.Platform.Application.Commands.SetUserPassword;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.Platform.Infrastructure.Authentication;
using RegOS.Platform.Infrastructure.Repositories;
using RegOS.Platform.Infrastructure.Services;
using RegOS.SharedKernel.Exceptions;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Tests.Commands.Login;

/// <summary>
/// Integration, against real Postgres and the real hasher and token issuer.
/// Sign-in is composition, so faking any part of it would test the fake.
/// </summary>
public sealed class LoginHandlerTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private const string CorrectPassword = "correct horse battery";

    private readonly OrganizationId _organizationId =
        OrganizationId.From(Guid.NewGuid());

    private readonly string _email =
        $"login.{Guid.NewGuid():N}@policy.example";

    private UserAggregate _user = default!;

    private static RegOSDbContext NewContext() =>
        new(new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString)
            .Options);

    private static JwtAccessTokenIssuer NewIssuer() =>
        new(Options.Create(new JwtOptions
        {
            SigningKey = "test-only-signing-key-at-least-32-bytes-long!",
            Issuer = "regos-tests",
            Audience = "regos-tests",
            AccessTokenMinutes = 15
        }));

    private static LoginHandler NewHandler(RegOSDbContext context) =>
        new(NewIssuer(),
            new PasswordHasher(),
            new UserCredentialRepository(context),
            new UserRepository(context));

    public async Task InitializeAsync()
    {
        await using var context = NewContext();

        _user = UserAggregate.Create(
            _organizationId, Email.Create(_email), "Login", "User");

        _user.Activate();

        context.Users.Add(_user);
        await context.SaveChangesAsync();

        await new SetUserPasswordHandler(
                new PasswordHasher(),
                new UserCredentialRepository(context),
                new UserRepository(context))
            .HandleAsync(
                new SetUserPasswordCommand(_user.Id, CorrectPassword),
                CancellationToken.None);
    }

    public async Task DisposeAsync()
    {
        await using var context = NewContext();

        // Users only: credentials cascade (ADR-023). This fixture once leaked
        // four orphaned credentials because it deleted users by organization
        // and credentials one at a time; that is now impossible to get wrong.
        await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Users\" WHERE \"OrganizationId\" = {0}",
            _organizationId.Value);
    }

    private async Task<LoginResult> LoginAsync(string email, string password)
    {
        await using var context = NewContext();

        return await NewHandler(context).HandleAsync(
            new LoginCommand(email, password),
            CancellationToken.None);
    }

    private async Task ShouldFailAsync(string email, string password)
    {
        var act = () => LoginAsync(email, password);

        // Every failure is indistinguishable: same type, same message. That is
        // the whole point of ADR-022.
        (await act.Should().ThrowAsync<AuthenticationFailedException>())
            .WithMessage(AuthenticationErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Issues_a_token_for_valid_credentials()
    {
        var result = await LoginAsync(_email, CorrectPassword);

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Issues_a_token_that_expires_in_the_future()
    {
        var result = await LoginAsync(_email, CorrectPassword);

        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Accepts_an_email_in_any_casing()
    {
        // Email normalizes, so sign-in must not be case sensitive.
        var result = await LoginAsync(_email.ToUpperInvariant(), CorrectPassword);

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public Task Rejects_a_wrong_password() =>
        ShouldFailAsync(_email, "not the password");

    [Fact]
    public Task Rejects_an_unknown_email() =>
        ShouldFailAsync($"absent.{Guid.NewGuid():N}@policy.example", CorrectPassword);

    [Fact]
    public Task Rejects_a_malformed_email() =>
        ShouldFailAsync("not-an-email", CorrectPassword);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public Task Rejects_an_empty_password(string password) =>
        ShouldFailAsync(_email, password);

    [Fact]
    public async Task Rejects_a_user_who_has_no_credential()
    {
        await using var context = NewContext();

        var withoutCredential = UserAggregate.Create(
            _organizationId,
            Email.Create($"nocred.{Guid.NewGuid():N}@policy.example"),
            "No",
            "Credential");

        withoutCredential.Activate();

        context.Users.Add(withoutCredential);
        await context.SaveChangesAsync();

        await ShouldFailAsync(withoutCredential.Email.Value, CorrectPassword);
    }

    [Fact]
    public async Task Rejects_a_deactivated_user_who_knows_the_password()
    {
        await using (var context = NewContext())
        {
            var user = await context.Users.SingleAsync(x => x.Id == _user.Id);
            user.Deactivate();
            await context.SaveChangesAsync();
        }

        // Knowing the password is not enough: an account that has been
        // deactivated must not be able to sign in.
        await ShouldFailAsync(_email, CorrectPassword);
    }

    [Fact]
    public async Task Rejects_an_invited_user_who_has_not_been_activated()
    {
        await using var context = NewContext();

        var invited = UserAggregate.Create(
            _organizationId,
            Email.Create($"invited.{Guid.NewGuid():N}@policy.example"),
            "Invited",
            "User");

        context.Users.Add(invited);
        await context.SaveChangesAsync();

        await new SetUserPasswordHandler(
                new PasswordHasher(),
                new UserCredentialRepository(context),
                new UserRepository(context))
            .HandleAsync(
                new SetUserPasswordCommand(invited.Id, CorrectPassword),
                CancellationToken.None);

        invited.Status.Should().Be(UserStatus.Invited);

        await ShouldFailAsync(invited.Email.Value, CorrectPassword);
    }

    [Fact]
    public async Task Issues_a_distinct_token_each_time()
    {
        // The jti claim differs per token, so two sign-ins are distinguishable
        // once revocation exists.
        var first = await LoginAsync(_email, CorrectPassword);
        var second = await LoginAsync(_email, CorrectPassword);

        second.AccessToken.Should().NotBe(first.AccessToken);
    }

    [Fact]
    public async Task Does_not_change_the_stored_hash_on_a_normal_login()
    {
        var before = await StoredHashAsync();

        await LoginAsync(_email, CorrectPassword);

        // Rehashing happens only when the hasher asks for it, not on every
        // sign-in.
        (await StoredHashAsync()).Should().Be(before);
    }

    private async Task<string> StoredHashAsync()
    {
        await using var context = NewContext();

        var credential = await context.UserCredentials
            .AsNoTracking()
            .SingleAsync(x => x.Id == _user.Id);

        return credential.PasswordHash;
    }
}
