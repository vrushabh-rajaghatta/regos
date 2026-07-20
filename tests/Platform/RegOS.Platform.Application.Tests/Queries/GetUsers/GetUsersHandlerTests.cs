using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.Platform.Application.Queries.GetUsers;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;
using RegOS.Platform.Application.Tests.Fakes;

namespace RegOS.Platform.Application.Tests.Queries.GetUsers;

// Integration tests - the user directory is a database projection, so it is
// exercised against the real dev Postgres (docker postgres-local) like the
// Submission application tests. Every test is scoped to a throwaway
// OrganizationId so it cannot collide with existing data.
public sealed class GetUsersHandlerTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private readonly OrganizationId _organizationId =
        OrganizationId.From(Guid.NewGuid());

    private static DbContextOptions<RegOSDbContext> Options() =>
        new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

    private static RegOSDbContext NewContext() => new(Options());

    public async Task InitializeAsync()
    {
        await using var context = NewContext();

        var ada = UserAggregate.Create(
            _organizationId, Email.Create("ada.lovelace@test.example"), "Ada", "Lovelace");

        var grace = UserAggregate.Create(
            _organizationId, Email.Create("grace.hopper@test.example"), "Grace", "Hopper");

        var alan = UserAggregate.Create(
            _organizationId, Email.Create("alan.turing@test.example"), "Alan", "Turing");

        alan.Activate();

        context.Users.AddRange(ada, grace, alan);

        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = NewContext();

        await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Users\" WHERE \"OrganizationId\" = {0}",
            _organizationId.Value);
    }

    /// <summary>
    /// The tenant is supplied to the handler, never to the query - there is no
    /// longer any way for a query to widen its own scope.
    /// </summary>
    private async Task<Common.PagedResult<UserListItem>> QueryAsync(
        GetUsersQuery query,
        OrganizationId? tenant = null)
    {
        await using var context = NewContext();

        return await new GetUsersHandler(
                context, new FakeTenantContext(tenant ?? _organizationId))
            .HandleAsync(query, CancellationToken.None);
    }

    [Fact]
    public async Task Returns_first_page_with_total_count()
    {
        var result = await QueryAsync(new GetUsersQuery());

        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Orders_newest_first()
    {
        var result = await QueryAsync(new GetUsersQuery());

        result.Items.Should().BeInDescendingOrder(x => x.CreatedOn);
    }

    [Fact]
    public async Task Applies_search_across_last_name()
    {
        var result = await QueryAsync(
            new GetUsersQuery(Search: "hopper"));

        result.TotalCount.Should().Be(1);
        result.Items.Single().LastName.Should().Be("Hopper");
    }

    [Fact]
    public async Task Applies_search_across_email()
    {
        var result = await QueryAsync(
            new GetUsersQuery(Search: "alan.turing@"));

        result.TotalCount.Should().Be(1);
        result.Items.Single().FirstName.Should().Be("Alan");
    }

    [Fact]
    public async Task Search_is_case_insensitive()
    {
        var result = await QueryAsync(
            new GetUsersQuery(Search: "LOVELACE"));

        result.Items.Single().LastName.Should().Be("Lovelace");
    }

    [Fact]
    public async Task Applies_status_filter()
    {
        var active = await QueryAsync(
            new GetUsersQuery(Status: UserStatus.Active));

        active.TotalCount.Should().Be(1);
        active.Items.Single().FirstName.Should().Be("Alan");

        var invited = await QueryAsync(
            new GetUsersQuery(Status: UserStatus.Invited));

        invited.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Returns_empty_collection_when_nothing_matches()
    {
        var result = await QueryAsync(
            new GetUsersQuery(Search: "no-such-person"));

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Respects_page_size_and_keeps_total_count()
    {
        var firstPage = await QueryAsync(
            new GetUsersQuery(Page: 1, PageSize: 2));

        firstPage.Items.Should().HaveCount(2);
        firstPage.TotalCount.Should().Be(3);

        var secondPage = await QueryAsync(
            new GetUsersQuery(Page: 2, PageSize: 2));

        secondPage.Items.Should().HaveCount(1);
        secondPage.TotalCount.Should().Be(3);

        firstPage.Items.Select(x => x.Id)
            .Should().NotIntersectWith(secondPage.Items.Select(x => x.Id));
    }

    [Theory]
    [InlineData(5000, GetUsersQuery.MaxPageSize)]
    [InlineData(0, 1)]
    public async Task Clamps_page_size(int requested, int expected)
    {
        var result = await QueryAsync(
            new GetUsersQuery(PageSize: requested));

        result.PageSize.Should().Be(expected);
    }

    [Fact]
    public async Task Defaults_invalid_page_to_first_page()
    {
        var result = await QueryAsync(
            new GetUsersQuery(Page: 0));

        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task Scopes_results_to_the_callers_tenant()
    {
        var other = await QueryAsync(
            new GetUsersQuery(), tenant: OrganizationId.From(Guid.NewGuid()));

        other.Items.Should().BeEmpty();
        other.TotalCount.Should().Be(0);
    }
}
