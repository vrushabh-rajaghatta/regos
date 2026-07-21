using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.Platform.Application.Authentication;
using RegOS.Platform.Application.Commands.ChangePassword;
using RegOS.Platform.Application.Commands.Login;
using RegOS.Platform.Application.Commands.SetUserPassword;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Application.Tests.Fakes;
using RegOS.Platform.Domain.Aggregates.PasswordReset;
using RegOS.Platform.Domain.Aggregates.Session;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.Platform.Infrastructure.Authentication;
using RegOS.Platform.Infrastructure.Repositories;
using RegOS.Platform.Infrastructure.Services;
using RegOS.SharedKernel.Exceptions;

using PasswordResetAggregate =
    RegOS.Platform.Domain.Aggregates.PasswordReset.PasswordReset;
using SessionAggregate = RegOS.Platform.Domain.Aggregates.Session.Session;
using RefreshTokenAggregate =
    RegOS.Platform.Domain.Aggregates.RefreshToken.RefreshToken;
using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Tests.Commands.ChangePassword;

/// <summary>
/// Integration, against real Postgres and the real hasher — like
/// <c>LoginHandlerTests</c>, and for the same reason: changing a password is
/// composition, and faking the hasher or the repositories would test the fakes.
/// Only <c>ICurrentUser</c> is a double, because the real one reads claims off
/// an HttpContext that belongs to the host.
/// </summary>
public sealed class ChangePasswordHandlerTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private const string OriginalPassword = "the original password";
    private const string NewPassword = "a brand new password";

    private readonly OrganizationId _organizationId =
        OrganizationId.From(Guid.NewGuid());

    private readonly string _email =
        $"changepassword.{Guid.NewGuid():N}@policy.example";

    private UserAggregate _user = default!;

    private static RegOSDbContext NewContext() =>
        new(new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString)
            .Options);

    public async Task InitializeAsync()
    {
        await using var context = NewContext();

        _user = UserAggregate.Create(
            _organizationId, Email.Create(_email), "Change", "Password");

        _user.Activate();

        context.Users.Add(_user);
        await context.SaveChangesAsync();

        await NewSetPassword(context).HandleAsync(
            new SetUserPasswordCommand(_user.Id, OriginalPassword),
            CancellationToken.None);
    }

    /// <summary>Users only: everything else cascades (ADR-026).</summary>
    public async Task DisposeAsync()
    {
        await using var context = NewContext();

        await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Users\" WHERE \"OrganizationId\" = {0}",
            _organizationId.Value);
    }

    private static SetUserPasswordHandler NewSetPassword(RegOSDbContext context) =>
        new(new PasswordHasher(),
            new UserCredentialRepository(context),
            new UserRepository(context));

    private ChangePasswordHandler NewHandler(RegOSDbContext context) =>
        new(NewSetPassword(context),
            new CredentialTrustRevoker(
                new SessionRevoker(
                    new RefreshTokenRepository(context),
                    new SessionRepository(context)),
                new PasswordResetRepository(context)),
            new FakeCurrentUser(
                _user.Id, _organizationId, Email.Create(_email)),
            new PasswordHasher(),
            new UserCredentialRepository(context),
            new UserRepository(context));

    private async Task ChangeAsync(
        string? current = OriginalPassword, string? next = NewPassword)
    {
        await using var context = NewContext();

        await NewHandler(context).HandleAsync(
            new ChangePasswordCommand(current, next), CancellationToken.None);
    }

    /// <summary>A live session, as sign-in would have left behind.</summary>
    private async Task<RefreshTokenAggregate> GiveThemASessionAsync()
    {
        await using var context = NewContext();

        // A session and the token carrying it, as sign-in would have left.
        var session = SessionAggregate.Start(
            _user.Id, "spec-agent", "127.0.0.1",
            DateTime.UtcNow.AddDays(14), DateTime.UtcNow);

        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        var token = RefreshTokenAggregate.Issue(
            _user.Id, session.Id, $"HASH-{Guid.NewGuid():N}",
            DateTime.UtcNow.AddDays(14), DateTime.UtcNow);

        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        return token;
    }

    /// <summary>An outstanding reset link, as "forgot password" would have.</summary>
    private async Task<PasswordResetAggregate> GiveThemAResetLinkAsync()
    {
        await using var context = NewContext();

        var reset = PasswordResetAggregate.Issue(
            _user.Id, $"HASH-{Guid.NewGuid():N}",
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow);

        context.PasswordResets.Add(reset);
        await context.SaveChangesAsync();

        return reset;
    }

    private static async Task<bool> IsRevokedAsync(RefreshTokenAggregate token)
    {
        await using var context = NewContext();

        return await context.RefreshTokens
            .Where(x => x.Id == token.Id)
            .Select(x => x.RevokedOn != null)
            .SingleAsync();
    }

    private static async Task<bool> IsRevokedAsync(PasswordResetAggregate reset)
    {
        await using var context = NewContext();

        return await context.PasswordResets
            .Where(x => x.Id == reset.Id)
            .Select(x => x.RevokedOn != null)
            .SingleAsync();
    }

    private async Task<bool> CanSignInWithAsync(string password)
    {
        await using var context = NewContext();

        var credential = await new UserCredentialRepository(context)
            .GetByUserIdAsync(_user.Id, CancellationToken.None);

        return new PasswordHasher().Verify(credential!.PasswordHash, password)
            != PasswordVerification.Failed;
    }

    // -------------------------------------------------------------- replacing

    [Fact]
    public async Task Replaces_the_password()
    {
        await ChangeAsync();

        (await CanSignInWithAsync(NewPassword)).Should().BeTrue();
        (await CanSignInWithAsync(OriginalPassword)).Should().BeFalse();
    }

    [Fact]
    public async Task Accepts_the_same_password_again()
    {
        // No "must differ from the current one" rule. Password validity is the
        // Password value object's business, and reuse policy is a product
        // feature nobody has asked for - inventing it here would be the kind of
        // quiet rule that surprises people later.
        var act = () => ChangeAsync(next: OriginalPassword);

        await act.Should().NotThrowAsync();
        (await CanSignInWithAsync(OriginalPassword)).Should().BeTrue();
    }

    [Fact]
    public async Task Rejects_a_new_password_that_breaks_the_rules()
    {
        var act = () => ChangeAsync(next: "short");

        await act.Should().ThrowAsync<DomainException>();

        // And nothing was half-applied.
        (await CanSignInWithAsync(OriginalPassword)).Should().BeTrue();
    }

    // ------------------------------------------------- proving you may do this

    [Fact]
    public async Task Rejects_an_incorrect_current_password()
    {
        var act = () => ChangeAsync(current: "not the current password");

        // A DomainException (400), not AuthenticationFailedException (401).
        // The caller is authenticated; answering 401 tells a client to
        // re-authenticate and ours duly threw the user out (ADR-028).
        (await act.Should().ThrowAsync<DomainException>())
            .WithMessage(AuthenticationErrors.IncorrectCurrentPassword);

        (await CanSignInWithAsync(OriginalPassword)).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Rejects_a_missing_current_password(string? current)
    {
        var act = () => ChangeAsync(current);

        (await act.Should().ThrowAsync<DomainException>())
            .WithMessage(AuthenticationErrors.IncorrectCurrentPassword);

        (await CanSignInWithAsync(OriginalPassword)).Should().BeTrue();
    }

    [Fact]
    public async Task Rejects_a_user_deactivated_since_their_token_was_issued()
    {
        // An access token outlives a deactivation by up to fifteen minutes, so
        // being authenticated is not the same as being allowed.
        await using (var context = NewContext())
        {
            _user.Deactivate();
            context.Users.Update(_user);
            await context.SaveChangesAsync();
        }

        var act = () => ChangeAsync();

        (await act.Should().ThrowAsync<AuthenticationFailedException>())
            .WithMessage(AuthenticationErrors.InvalidCredentials);

        (await CanSignInWithAsync(OriginalPassword)).Should().BeTrue();
    }

    // ------------------------------------------------------------- ADR-028

    [Fact]
    public async Task Ends_every_session_including_the_one_making_the_request()
    {
        var session = await GiveThemASessionAsync();
        var other = await GiveThemASessionAsync();

        await ChangeAsync();

        (await IsRevokedAsync(session)).Should().BeTrue();
        (await IsRevokedAsync(other)).Should().BeTrue();
    }

    [Fact]
    public async Task Revokes_an_outstanding_password_reset_link()
    {
        // The one that is easy to miss. Someone reading the user's mailbox asks
        // for a reset; the user notices and changes their password. If the link
        // survived, changing the password would have achieved nothing - the
        // attacker still holds a way to set one of their own.
        var reset = await GiveThemAResetLinkAsync();

        await ChangeAsync();

        (await IsRevokedAsync(reset)).Should().BeTrue();
    }

    [Fact]
    public async Task Revokes_nothing_when_the_change_is_refused()
    {
        var session = await GiveThemASessionAsync();
        var reset = await GiveThemAResetLinkAsync();

        var act = () => ChangeAsync(current: "not the current password");

        await act.Should().ThrowAsync<DomainException>();

        // Otherwise a stranger with a stolen access token could sign everyone
        // out and kill their recovery link without knowing any password.
        (await IsRevokedAsync(session)).Should().BeFalse();
        (await IsRevokedAsync(reset)).Should().BeFalse();
    }
}
