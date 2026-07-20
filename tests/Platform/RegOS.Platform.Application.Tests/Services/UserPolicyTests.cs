using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.Platform.Infrastructure.Services;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Application.Tests.Services;

// Integration tests for the uniqueness rules — the exclude-self behaviour is
// SQL, so fakes cannot prove it. Scoped to a throwaway OrganizationId.
public sealed class UserPolicyTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private readonly OrganizationId _organizationId =
        OrganizationId.From(Guid.NewGuid());

    private UserAggregate _existing = default!;
    private UserAggregate _other = default!;

    private static RegOSDbContext NewContext() =>
        new(new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString)
            .Options);

    public async Task InitializeAsync()
    {
        await using var context = NewContext();

        _existing = UserAggregate.Create(
            _organizationId, Email.Create("taken@policy.example"), "Taken", "User");

        _other = UserAggregate.Create(
            _organizationId, Email.Create("other@policy.example"), "Other", "User");

        context.Users.AddRange(_existing, _other);

        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = NewContext();

        await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Users\" WHERE \"OrganizationId\" = {0}",
            _organizationId.Value);
    }

    [Fact]
    public async Task Update_allows_a_user_to_keep_its_own_email()
    {
        await using var context = NewContext();
        var policy = new UserPolicy(context);

        var act = () => policy.EnsureEmailIsUniqueForUpdateAsync(
            _organizationId,
            _existing.Id,
            Email.Create("taken@policy.example"),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Update_rejects_an_email_owned_by_another_user()
    {
        await using var context = NewContext();
        var policy = new UserPolicy(context);

        var act = () => policy.EnsureEmailIsUniqueForUpdateAsync(
            _organizationId,
            _other.Id,
            Email.Create("taken@policy.example"),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task Update_allows_a_genuinely_new_email()
    {
        await using var context = NewContext();
        var policy = new UserPolicy(context);

        var act = () => policy.EnsureEmailIsUniqueForUpdateAsync(
            _organizationId,
            _existing.Id,
            Email.Create("brand.new@policy.example"),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
