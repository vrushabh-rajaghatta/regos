using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Platform.Application.Tests.Fakes;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Primitives;

using RegOS.Organization.Domain.Aggregates.Organization;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;
using OrganizationAggregate =
    RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Platform.Application.Tests.Tenancy;

/// <summary>
/// Proves the global query filter does the isolating on its own (ADR-031).
/// Every query here is deliberately bare — no handler, no manual
/// <c>.Where(TenantId == …)</c> — because the claim under test is exactly
/// that forgetting the manual clause no longer leaks.
/// </summary>
public sealed class TenantIsolationTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private readonly TenantId _tenantA = TenantId.From(Guid.NewGuid());
    private readonly TenantId _tenantB = TenantId.From(Guid.NewGuid());

    private UserAggregate _userInA = default!;
    private UserAggregate _userInB = default!;
    private UserAggregate _platformUser = default!;

    private static DbContextOptions<RegOSDbContext> Options() =>
        new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

    private RegOSDbContext ContextFor(TenantId tenant) =>
        new(Options(), new FakeTenantContext(tenant));

    private static RegOSDbContext ContextWithoutIdentity() =>
        new(Options());

    public async Task InitializeAsync()
    {
        _userInA = UserAggregate.CreateForTenant(
            _tenantA, UniqueEmail("a"), "Ada", "Alpha");
        _userInB = UserAggregate.CreateForTenant(
            _tenantB, UniqueEmail("b"), "Bea", "Beta");
        _platformUser = UserAggregate.CreatePlatformUser(
            UniqueEmail("p"), "Platform", "Person");

        await using var context = ContextWithoutIdentity();
        context.Users.AddRange(_userInA, _userInB, _platformUser);
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = ContextWithoutIdentity();

        // Raw SQL: cleanup must not depend on the very filters under test.
        await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Users\" WHERE \"Id\" IN ({0}, {1}, {2})",
            _userInA.Id.Value, _userInB.Id.Value, _platformUser.Id.Value);
    }

    private static Email UniqueEmail(string prefix) =>
        Email.Create($"{prefix}.{Guid.NewGuid():N}@isolation.test.example");

    [Fact]
    public async Task A_bare_query_returns_only_the_callers_tenant()
    {
        await using var context = ContextFor(_tenantA);

        // No .Where at all — the filter is the only thing scoping this.
        var visible = await context.Users
            .Select(x => x.Id)
            .ToListAsync();

        visible.Should().Contain(_userInA.Id);
        visible.Should().NotContain(_userInB.Id);
    }

    [Fact]
    public async Task The_other_tenant_sees_the_mirror_image()
    {
        await using var context = ContextFor(_tenantB);

        var visible = await context.Users
            .Select(x => x.Id)
            .ToListAsync();

        visible.Should().Contain(_userInB.Id);
        visible.Should().NotContain(_userInA.Id);
    }

    [Fact]
    public async Task No_identity_means_no_rows_not_all_rows()
    {
        await using var context = ContextWithoutIdentity();

        // Fail closed: rows exist (InitializeAsync put them there), and an
        // identityless context must see none of them. This is the test that
        // catches the SQL-null-semantics trap — with Users.TenantId nullable,
        // a bare equality filter would translate a null tenant into
        // "TenantId IS NULL" and hand back every platform user.
        var count = await context.Users.CountAsync();

        count.Should().Be(0);
    }

    [Fact]
    public async Task A_platform_user_is_invisible_to_every_tenant()
    {
        await using var contextA = ContextFor(_tenantA);
        await using var contextB = ContextFor(_tenantB);

        (await contextA.Users.AnyAsync(x => x.Id == _platformUser.Id))
            .Should().BeFalse();
        (await contextB.Users.AnyAsync(x => x.Id == _platformUser.Id))
            .Should().BeFalse();
    }

    [Fact]
    public async Task An_organization_belongs_to_one_registry()
    {
        // ADR-032: even the *names* in another tenant's registry are
        // competitively sensitive. Created bare, queried bare — the filter
        // does all the isolating.
        var org = OrganizationAggregate.Create(
            _tenantA, $"Registry Test {Guid.NewGuid():N}", OrganizationType.Manufacturer);

        await using (var contextA = ContextFor(_tenantA))
        {
            contextA.Organizations.Add(org);
            await contextA.SaveChangesAsync();
        }

        try
        {
            await using var contextA = ContextFor(_tenantA);
            await using var contextB = ContextFor(_tenantB);

            (await contextA.Organizations.AnyAsync(x => x.Id == org.Id))
                .Should().BeTrue();
            (await contextB.Organizations.AnyAsync(x => x.Id == org.Id))
                .Should().BeFalse(
                    "tenant B must not even learn the organization exists");
        }
        finally
        {
            await using var cleanup = ContextWithoutIdentity();
            await cleanup.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"Organizations\" WHERE \"Id\" = {0}",
                org.Id.Value);
        }
    }

    [Fact]
    public async Task The_directory_read_model_is_filtered_like_its_table()
    {
        // UserDirectoryRow is a different CLR type over the same Users table;
        // the aggregate's filter does not propagate to it, so this asserts
        // its own filter exists and agrees.
        await using var context = ContextFor(_tenantA);

        var visible = await context.UserDirectory
            .Select(x => x.Id)
            .ToListAsync();

        visible.Should().Contain(_userInA.Id.Value);
        visible.Should().NotContain(_userInB.Id.Value);
        visible.Should().NotContain(_platformUser.Id.Value);
    }
}
