using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.SharedKernel.Primitives;
using RegOS.Persistence;
using RegOS.Platform.Application.Commands.SetUserPassword;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.Platform.Infrastructure.Repositories;
using RegOS.Platform.Infrastructure.Services;
using RegOS.SharedKernel.Exceptions;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Application.Tests.Commands.SetUserPassword;

/// <summary>
/// Integration, not unit: the point of this slice is that a password survives a
/// round trip through real Postgres and still verifies. A faked hasher or an
/// in-memory store would prove nothing about that.
/// </summary>
public sealed class SetUserPasswordHandlerTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private const string CorrectPassword = "correct horse battery";

    private readonly TenantId _tenantId =
        TenantId.From(Guid.NewGuid());

    private UserAggregate _user = default!;

    private static RegOSDbContext NewContext() =>
        new(new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString)
            .Options);

    private static SetUserPasswordHandler NewHandler(RegOSDbContext context) =>
        new(new PasswordHasher(),
            new UserCredentialRepository(context),
            new UserRepository(context));

    public async Task InitializeAsync()
    {
        await using var context = NewContext();

        _user = UserAggregate.CreateForTenant(
            _tenantId,
            Email.Create($"credential.{Guid.NewGuid():N}@policy.example"),
            "Credential",
            "User");

        context.Users.Add(_user);

        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = NewContext();

        // Users only: credentials cascade (ADR-026). Deleting them explicitly
        // would still work, but it would hide whether the constraint is doing
        // its job — Cascades_the_credential_when_the_user_is_deleted asserts it.
        await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Users\" WHERE \"TenantId\" = {0}",
            _tenantId.Value);
    }

    private async Task SetPasswordAsync(string password)
    {
        await using var context = NewContext();

        await NewHandler(context).HandleAsync(
            new SetUserPasswordCommand(_user.Id, password),
            CancellationToken.None);
    }

    private async Task<string> StoredHashAsync()
    {
        await using var context = NewContext();

        var credential = await context.UserCredentials
            .AsNoTracking()
            .SingleAsync(x => x.Id == _user.Id);

        return credential.PasswordHash;
    }

    [Fact]
    public async Task Stores_a_credential_for_the_user()
    {
        await SetPasswordAsync(CorrectPassword);

        (await StoredHashAsync()).Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Never_stores_the_password_itself()
    {
        await SetPasswordAsync(CorrectPassword);

        (await StoredHashAsync()).Should().NotContain(CorrectPassword);
    }

    [Fact]
    public async Task Verifies_the_correct_password_after_a_round_trip()
    {
        await SetPasswordAsync(CorrectPassword);

        var result = new PasswordHasher()
            .Verify(await StoredHashAsync(), CorrectPassword);

        result.Should().Be(PasswordVerification.Succeeded);
    }

    [Fact]
    public async Task Rejects_an_incorrect_password()
    {
        await SetPasswordAsync(CorrectPassword);

        var result = new PasswordHasher()
            .Verify(await StoredHashAsync(), "not the password");

        result.Should().Be(PasswordVerification.Failed);
    }

    [Fact]
    public async Task Produces_a_different_hash_for_the_same_password()
    {
        // Salting, verified rather than assumed: two users with identical
        // passwords must not share a hash, or the store leaks who shares one.
        await SetPasswordAsync(CorrectPassword);
        var first = await StoredHashAsync();

        await SetPasswordAsync(CorrectPassword);
        var second = await StoredHashAsync();

        second.Should().NotBe(first);
    }

    [Fact]
    public async Task Replaces_the_credential_when_the_password_is_set_again()
    {
        await SetPasswordAsync(CorrectPassword);
        await SetPasswordAsync("a completely different one");

        var hasher = new PasswordHasher();
        var hash = await StoredHashAsync();

        hasher.Verify(hash, "a completely different one")
            .Should().Be(PasswordVerification.Succeeded);

        hasher.Verify(hash, CorrectPassword)
            .Should().Be(PasswordVerification.Failed);
    }

    [Fact]
    public async Task Keeps_exactly_one_credential_per_user()
    {
        await SetPasswordAsync(CorrectPassword);
        await SetPasswordAsync("another password entirely");

        await using var context = NewContext();

        var count = await context.UserCredentials
            .CountAsync(x => x.Id == _user.Id);

        count.Should().Be(1);
    }

    [Fact]
    public async Task Cascades_the_credential_when_the_user_is_deleted()
    {
        // ADR-026: a credential has no meaning without its user, so the schema
        // refuses to keep one. Asserted against the real database because this
        // is a constraint, not a code path.
        await SetPasswordAsync(CorrectPassword);

        await using var context = NewContext();

        await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Users\" WHERE \"Id\" = {0}",
            _user.Id.Value);

        var remaining = await context.UserCredentials
            .CountAsync(x => x.Id == _user.Id);

        remaining.Should().Be(0);
    }

    [Fact]
    public async Task Rejects_a_password_that_fails_the_domain_rules()
    {
        var act = () => SetPasswordAsync("short");

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage(PasswordErrors.TooShort);
    }

    [Fact]
    public async Task Rejects_setting_a_password_for_a_user_that_does_not_exist()
    {
        await using var context = NewContext();

        var act = () => NewHandler(context).HandleAsync(
            new SetUserPasswordCommand(UserId.New(), CorrectPassword),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Validates_the_password_before_looking_up_the_user()
    {
        // A weak password on an unknown user is still a 400, not a 404: the
        // rule is decidable from the request alone (ADR-009), and answering
        // 404 first would confirm which accounts exist.
        await using var context = NewContext();

        var act = () => NewHandler(context).HandleAsync(
            new SetUserPasswordCommand(UserId.New(), "short"),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
