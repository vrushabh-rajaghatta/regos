using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.SharedKernel.Primitives;
using RegOS.Persistence;
using RegOS.Platform.Application.Queries.GetUserById;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;
using RegOS.SharedKernel.Exceptions;
using RegOS.Platform.Application.Tests.Fakes;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Application.Tests.Queries.GetUserById;

// Integration tests against the real dev Postgres, scoped to a throwaway
// TenantId so they cannot collide with existing data.
public sealed class GetUserByIdHandlerTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private readonly TenantId _tenantId =
        TenantId.From(Guid.NewGuid());

    private readonly TenantId _otherTenantId =
        TenantId.From(Guid.NewGuid());

    private UserId _userId = default!;

    private static DbContextOptions<RegOSDbContext> Options() =>
        new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

    // The context carries the same tenant the handler is scoped to, so the
    // global query filter (ADR-031) resolves to this test's rows.
    private RegOSDbContext NewContext() =>
        new(Options(), new FakeTenantContext(_tenantId));

    public async Task InitializeAsync()
    {
        await using var context = NewContext();

        var user = UserAggregate.CreateForTenant(
            _tenantId,
            Email.Create("grace.hopper@details.example"),
            "Grace",
            "Hopper");

        _userId = user.Id;

        context.Users.Add(user);

        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = NewContext();

        await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Users\" WHERE \"TenantId\" = {0}",
            _tenantId.Value);
    }

    private async Task<UserDetails> QueryAsync(
        GetUserByIdQuery query,
        TenantId? tenant = null)
    {
        await using var context = NewContext();

        return await new GetUserByIdHandler(
                context, new FakeTenantContext(tenant ?? _tenantId))
            .HandleAsync(query, CancellationToken.None);
    }

    [Fact]
    public async Task Returns_the_user_when_found()
    {
        var user = await QueryAsync(
            new GetUserByIdQuery(_userId));

        user.Id.Should().Be(_userId.Value);
    }

    [Fact]
    public async Task Projects_every_field_correctly()
    {
        var user = await QueryAsync(
            new GetUserByIdQuery(_userId));

        user.FirstName.Should().Be("Grace");
        user.LastName.Should().Be("Hopper");
        user.Email.Should().Be("grace.hopper@details.example");
        user.Status.Should().Be(UserStatus.Invited);
        user.CreatedOn.Should().NotBe(default);
    }

    [Fact]
    public async Task Throws_not_found_when_the_user_does_not_exist()
    {
        var act = () => QueryAsync(
            new GetUserByIdQuery(UserId.New()));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_not_found_when_the_user_belongs_to_another_organization()
    {
        // Tenant isolation: an existing user outside the caller's organization
        // must be indistinguishable from one that does not exist.
        var act = () => QueryAsync(
            new GetUserByIdQuery(_userId), tenant: _otherTenantId);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
