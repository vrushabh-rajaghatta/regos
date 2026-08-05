using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Application.Commands.CreateOrganizationDivision;
using RegOS.Organization.Application.Queries.Divisions.ListOrganizationDivisions;
using RegOS.Organization.Application.Services;
using RegOS.Organization.Application.Tests.Fixtures;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationDivision;
using RegOS.Organization.Infrastructure.Persistence;
using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Organization.Application.Tests;

[Collection(OrganizationDatabase.Collection)]
public sealed class OrganizationDivisionTests : IAsyncLifetime
{
    private readonly OrganizationDatabase _database;

    public OrganizationDivisionTests(OrganizationDatabase database)
    {
        _database = database;
    }


    private static readonly IdentifierSchemeId Duns =
        new(Guid.Parse("80000000-0000-0000-0000-000000000001"));

    private static readonly DateOnly Established = new(2018, 4, 1);

    private readonly List<Guid> _divisionIds = [];
    private readonly List<Guid> _organizationIds = [];

    private RegOSDbContext New(ITenantContext? tenant = null) =>
        new(
            _database.Options,
            tenant ?? TestTenants.ActingContext);

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var ctx = New();

        foreach (var id in _divisionIds)
        {
            var division = await ctx.OrganizationDivisions
                .FirstOrDefaultAsync(x => x.Id == new OrganizationDivisionId(id));

            if (division is not null)
                ctx.OrganizationDivisions.Remove(division);
        }

        await ctx.SaveChangesAsync();

        foreach (var id in _organizationIds)
        {
            var organization = await ctx.Organizations
                .Include(x => x.Identifiers)
                .FirstOrDefaultAsync(x => x.Id == new OrganizationId(id));

            if (organization is not null)
                ctx.Organizations.Remove(organization);
        }

        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task ADivisionIsPersistedUnderItsOrganization()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);

        var id = await CreateAsync(ctx, organizationId);

        await using var check = New();
        var division = await check.OrganizationDivisions
            .AsNoTracking().FirstAsync(x => x.Id == id);

        division.TenantId.Should().Be(TestTenants.Acting);
        division.Status.Should().Be(OrganizationStatus.Active);
        division.StatusDate.Should().Be(Established);
    }

    [Fact]
    public async Task AnUnknownOrganizationIsNotFound()
    {
        await using var ctx = New();

        var create = async () => await CreateAsync(ctx, OrganizationId.New());

        await create.Should().ThrowAsync<NotFoundException>()
            .WithMessage(OrganizationSiteRuleErrors.OrganizationDoesNotExist);
    }

    /// <summary>
    /// A division is a root, so nothing else is protecting it — the same claim
    /// proved for sites and contacts.
    /// </summary>
    [Fact]
    public async Task AnotherTenantSeesNoneOfTheseDivisions()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);
        var id = await CreateAsync(ctx, organizationId);

        await using var intruder = New(TestTenants.OtherContext);

        (await intruder.OrganizationDivisions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id))
            .Should().BeNull();
    }

    [Fact]
    public async Task AnOrganizationListsItsDivisions()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);
        await CreateAsync(ctx, organizationId);

        await using var check = New();
        var rows = await new ListOrganizationDivisionsHandler(check)
            .HandleAsync(new ListOrganizationDivisionsQuery(organizationId), default);

        rows!.Should().ContainSingle().Which.Acronym.Should().Be("RA");
    }

    /// <summary>
    /// The identity attributes, round-tripped — including the identifier
    /// collection that deliberately duplicates SiteIdentifier.
    /// </summary>
    [Fact]
    public async Task AnOrganizationsIdentityIsPersisted()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);

        var organization = await ctx.Organizations
            .Include(x => x.Identifiers)
            .FirstAsync(x => x.Id == organizationId);

        organization.DescribeAs("DML", "デモ製薬株式会社");
        organization.AddIdentifier(Duns, "150483782");
        await ctx.SaveChangesAsync();

        await using var check = New();
        var reloaded = await check.Organizations
            .AsNoTracking()
            .Include(x => x.Identifiers)
            .FirstAsync(x => x.Id == organizationId);

        reloaded.Acronym.Should().Be("DML");
        reloaded.NameNativeLanguage.Should().Be("デモ製薬株式会社");
        reloaded.Identifiers.Should().ContainSingle()
            .Which.Value.Should().Be("150483782");
        reloaded.StatusDate.Should().NotBe(default);
    }

    private async Task<OrganizationDivisionId> CreateAsync(
        RegOSDbContext ctx,
        OrganizationId organizationId)
    {
        var handler = new CreateOrganizationDivisionHandler(
            ctx,
            new OrganizationDivisionRepository(ctx),
            TestTenants.ActingContext);

        var id = await handler.HandleAsync(
            new CreateOrganizationDivisionCommand(
                organizationId, "Regulatory Affairs", Established, "RA"),
            default);

        _divisionIds.Add(id.Value);

        return id;
    }

    private async Task<OrganizationId> OrganizationAsync(RegOSDbContext ctx)
    {
        var organization = OrganizationAggregate.Create(
            TestTenants.Acting,
            $"Division Partner {Guid.NewGuid():N}",
            OrganizationType.Manufacturer);

        ctx.Organizations.Add(organization);
        await ctx.SaveChangesAsync();
        _organizationIds.Add(organization.Id.Value);

        return organization.Id;
    }
}
