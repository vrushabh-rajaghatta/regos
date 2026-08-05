using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Application.Commands.CreateOrganizationSite;
using RegOS.Organization.Application.Queries.Sites.GetOrganizationSite;
using RegOS.Organization.Application.Queries.Sites.ListOrganizationSites;
using RegOS.Organization.Application.Queries.Sites.SiteDirectory;
using RegOS.Organization.Application.Services;
using RegOS.Organization.Application.Tests.Fixtures;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Organization.Infrastructure.Persistence;
using RegOS.Organization.Infrastructure.Services;
using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Organization.Application.Tests;

/// <summary>
/// Integration tests — sites against the real seeded reference data in the dev
/// Postgres.
/// </summary>
[Collection(OrganizationDatabase.Collection)]
public sealed class OrganizationSiteTests : IAsyncLifetime
{
    private readonly OrganizationDatabase _database;

    public OrganizationSiteTests(OrganizationDatabase database)
    {
        _database = database;
    }


    private static readonly CountryId India =
        new(Guid.Parse("10000000-0000-0000-0000-000000000004"));

    /// <summary>FEI — seeded by IdentifierSchemeDataInitializer.</summary>
    private static readonly IdentifierSchemeId Fei =
        new(Guid.Parse("80000000-0000-0000-0000-000000000002"));

    private static readonly IdentifierSchemeId Duns =
        new(Guid.Parse("80000000-0000-0000-0000-000000000001"));

    private static readonly DateOnly Opened = new(2014, 5, 1);

    private readonly List<Guid> _siteIds = [];
    private readonly List<Guid> _organizationIds = [];

    private RegOSDbContext New(ITenantContext? tenant = null) =>
        new(
            _database.Options,
            tenant ?? TestTenants.ActingContext);

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var ctx = New();

        foreach (var id in _siteIds)
        {
            var site = await ctx.OrganizationSites
                .Include(x => x.Identifiers)
                .FirstOrDefaultAsync(x => x.Id == new OrganizationSiteId(id));

            if (site is not null)
                ctx.OrganizationSites.Remove(site);
        }

        await ctx.SaveChangesAsync();

        foreach (var id in _organizationIds)
        {
            var organization = await ctx.Organizations
                .FirstOrDefaultAsync(x => x.Id == new OrganizationId(id));

            if (organization is not null)
                ctx.Organizations.Remove(organization);
        }

        await ctx.SaveChangesAsync();
    }

    // --- Creating ------------------------------------------------------------

    [Fact]
    public async Task ASiteIsPersistedWithItsAddressAndIdentifiers()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);

        var id = await CreateAsync(
            ctx,
            organizationId,
            identifiers:
            [
                new SiteIdentifierInput(Fei, "3001234567"),
                new SiteIdentifierInput(Duns, "150483782"),
            ]);

        await using var check = New();
        var site = await check.OrganizationSites
            .AsNoTracking()
            .Include(x => x.Identifiers)
            .FirstAsync(x => x.Id == id);

        site.TenantId.Should().Be(TestTenants.Acting);
        site.Status.Should().Be(OrganizationStatus.Active);
        site.StatusDate.Should().Be(Opened);
        site.Address.CountryId.Should().Be(India);
        site.Address.City.Should().Be("Hyderabad");
        site.Identifiers.Should().HaveCount(2);
    }

    /// <summary>
    /// The weak value object, proved against the database: a site whose only
    /// known address detail is its country still persists.
    /// </summary>
    [Fact]
    public async Task ASiteNeedsNothingButACountry()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);

        var id = await CreateAsync(ctx, organizationId, city: null);

        await using var check = New();
        var site = await check.OrganizationSites
            .AsNoTracking().FirstAsync(x => x.Id == id);

        site.Address.City.Should().BeNull();
        site.Address.Line1.Should().BeNull();
        site.Address.CountryId.Should().Be(India);
    }

    /// <summary>
    /// The aggregate refuses this, and so does the unique index behind it —
    /// the persistence model reinforces the domain model rather than merely
    /// storing it.
    /// </summary>
    [Fact]
    public async Task TwoIdentifiersFromOneSchemeAreRefused()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);

        var create = async () => await CreateAsync(
            ctx,
            organizationId,
            identifiers:
            [
                new SiteIdentifierInput(Fei, "3001234567"),
                new SiteIdentifierInput(Fei, "3009999999"),
            ]);

        await create.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(OrganizationSiteErrors.IdentifierSchemeAlreadyRecorded);
    }

    [Fact]
    public async Task AnUnknownOrganizationIsNotFound()
    {
        await using var ctx = New();

        var create = async () => await CreateAsync(ctx, OrganizationId.New());

        await create.Should().ThrowAsync<NotFoundException>()
            .WithMessage(OrganizationSiteRuleErrors.OrganizationDoesNotExist);
    }

    [Fact]
    public async Task AnInactiveOrganizationCannotGainSites()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx, active: false);

        var create = async () => await CreateAsync(ctx, organizationId);

        await create.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(OrganizationSiteRuleErrors.OrganizationInactive);
    }

    [Fact]
    public async Task AnUnknownCountryIsRejected()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);

        var create = async () => await CreateAsync(
            ctx, organizationId, countryId: new CountryId(Guid.NewGuid()));

        await create.Should().ThrowAsync<DomainException>()
            .WithMessage(OrganizationSiteRuleErrors.CountryDoesNotExist);
    }

    [Fact]
    public async Task AnUnknownIdentifierSchemeIsRejected()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);

        var create = async () => await CreateAsync(
            ctx,
            organizationId,
            identifiers:
            [
                new SiteIdentifierInput(
                    new IdentifierSchemeId(Guid.NewGuid()), "123"),
            ]);

        await create.Should().ThrowAsync<DomainException>()
            .WithMessage(
                OrganizationSiteRuleErrors.IdentifierSchemeDoesNotExist);
    }

    // --- Tenant isolation ----------------------------------------------------

    /// <summary>
    /// The claim that made sites carry their own filter rather than inherit the
    /// organization's: a site is a root, reachable directly through the
    /// directory, so nothing else is protecting it (ADR-031/032).
    /// </summary>
    [Fact]
    public async Task AnotherTenantSeesNoneOfTheseSites()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);
        var id = await CreateAsync(ctx, organizationId);

        await using var intruder = New(TestTenants.OtherContext);

        // Directly by id...
        (await intruder.OrganizationSites
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id))
            .Should().BeNull();

        // ...and through the directory, which is the route that made the site
        // a root in the first place.
        var directory = await new SiteDirectoryHandler(intruder)
            .HandleAsync(new SiteDirectoryQuery(India), default);

        directory.Should().NotContain(x => x.SiteId == id.Value);

        // The detail read model agrees: not visible is not found.
        (await new GetOrganizationSiteHandler(intruder).HandleAsync(new GetOrganizationSiteQuery(id), default))
            .Should().BeNull();
    }

    // --- The directory -------------------------------------------------------

    /// <summary>
    /// "Which manufacturing sites do we have in India?" — the query that
    /// justifies this being an aggregate root, and the reason it ships in the
    /// same story.
    /// </summary>
    [Fact]
    public async Task TheDirectoryAnswersByCountryAndType()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);

        var plant = await CreateAsync(ctx, organizationId);
        var lab = await CreateAsync(
            ctx, organizationId, type: OrganizationSiteType.Testing);

        await using var check = New();
        var handler = new SiteDirectoryHandler(check);

        var inIndia = await handler.HandleAsync(new SiteDirectoryQuery(India), default);
        inIndia.Should().Contain(x => x.SiteId == plant.Value);
        inIndia.Should().Contain(x => x.SiteId == lab.Value);

        var manufacturing = await handler.HandleAsync(
            new SiteDirectoryQuery(India, OrganizationSiteType.Manufacturing), default);

        manufacturing.Should().Contain(x => x.SiteId == plant.Value);
        manufacturing.Should().NotContain(x => x.SiteId == lab.Value);

        // The directory spans the registry, so it names the owning company.
        manufacturing.Single(x => x.SiteId == plant.Value)
            .OrganizationName.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Nothing is hidden: a site that closed last year is still the site named
    /// on a licence granted in 2019.
    /// </summary>
    [Fact]
    public async Task TheDirectoryKeepsClosedSitesAndMarksThem()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);
        var id = await CreateAsync(ctx, organizationId);

        await using var act = New();
        var repository = new OrganizationSiteRepository(act);
        var site = await repository.GetByIdAsync(id, default);
        site!.Deactivate(new DateOnly(2025, 3, 31));
        await repository.UpdateAsync(site, default);

        await using var check = New();
        var row = (await new SiteDirectoryHandler(check)
            .HandleAsync(new SiteDirectoryQuery(India), default))
            .Single(x => x.SiteId == id.Value);

        row.Status.Should().Be(nameof(OrganizationStatus.Inactive));
        row.StatusDate.Should().Be(new DateOnly(2025, 3, 31));
    }

    [Fact]
    public async Task AnOrganizationListsItsOwnSites()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);
        await CreateAsync(ctx, organizationId, identifiers:
            [new SiteIdentifierInput(Fei, "3001234567")]);

        await using var check = New();
        var sites = await new ListOrganizationSitesHandler(check)
            .HandleAsync(new ListOrganizationSitesQuery(organizationId), default);

        var row = sites!.Should().ContainSingle().Subject;

        row.CountryName.Should().NotBeNullOrWhiteSpace();
        row.Identifiers.Should().ContainSingle()
            .Which.SchemeCode.Should().Be("FEI");
    }

    [Fact]
    public async Task AnUnknownOrganizationHasNoSiteList()
    {
        await using var ctx = New();

        (await new ListOrganizationSitesHandler(ctx)
            .HandleAsync(new ListOrganizationSitesQuery(OrganizationId.New()), default))
            .Should().BeNull();
    }

    [Fact]
    public async Task TheDetailViewResolvesTheNamesAPersonReads()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);
        var id = await CreateAsync(ctx, organizationId, identifiers:
            [new SiteIdentifierInput(Duns, "150483782")]);

        await using var check = New();
        var detail = await new GetOrganizationSiteHandler(check)
            .HandleAsync(new GetOrganizationSiteQuery(id), default);

        detail.Should().NotBeNull();
        detail!.OrganizationName.Should().NotBeNullOrWhiteSpace();
        detail.Address.CountryName.Should().NotBeNullOrWhiteSpace();
        detail.Type.Should().Be(nameof(OrganizationSiteType.Manufacturing));
        detail.Identifiers.Should().ContainSingle()
            .Which.SchemeCode.Should().Be("DUNS");
    }

    // --- Seeded reference data -----------------------------------------------

    [Fact]
    public async Task TheIdentifierSchemesAreSeededAndGlobal()
    {
        await using var ctx = New();

        var codes = await ctx.IdentifierSchemes
            .AsNoTracking()
            .Select(x => x.Code)
            .ToListAsync();

        codes.Should().Contain(["DUNS", "FEI", "EU-ORG-ID", "SPL-ID"]);

        // A world fact, not a tenant's list — another tenant sees the same set.
        await using var other = New(TestTenants.OtherContext);
        (await other.IdentifierSchemes.AsNoTracking().CountAsync())
            .Should().Be(codes.Count);
    }

    // --- helpers -------------------------------------------------------------

    private async Task<OrganizationSiteId> CreateAsync(
        RegOSDbContext ctx,
        OrganizationId organizationId,
        OrganizationSiteType type = OrganizationSiteType.Manufacturing,
        CountryId? countryId = null,
        string? city = "Hyderabad",
        IReadOnlyList<SiteIdentifierInput>? identifiers = null)
    {
        var handler = new CreateOrganizationSiteHandler(
            new OrganizationSiteCreationPolicy(ctx),
            new OrganizationSiteRepository(ctx),
            TestTenants.ActingContext);

        var id = await handler.HandleAsync(
            new CreateOrganizationSiteCommand(
                organizationId,
                $"Site {Guid.NewGuid():N}",
                type,
                countryId ?? India,
                Opened,
                City: city,
                Identifiers: identifiers),
            default);

        _siteIds.Add(id.Value);

        return id;
    }

    private async Task<OrganizationId> OrganizationAsync(
        RegOSDbContext ctx,
        bool active = true)
    {
        var organization = OrganizationAggregate.Create(
            TestTenants.Acting,
            $"Site Partner {Guid.NewGuid():N}",
            OrganizationType.Manufacturer);

        if (!active)
            organization.Deactivate();

        ctx.Organizations.Add(organization);
        await ctx.SaveChangesAsync();
        _organizationIds.Add(organization.Id.Value);

        return organization.Id;
    }
}
