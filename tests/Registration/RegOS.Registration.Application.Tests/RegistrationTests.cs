using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Registration.Application.Commands.ChangeRegistrationStatus;
using RegOS.Registration.Application.Commands.CreateRegistration;
using RegOS.Registration.Application.Commands.RecordRegistrationApproval;
using RegOS.Registration.Application.Queries.GetRegistration;
using RegOS.Registration.Application.Queries.ListExpiringRegistrations;
using RegOS.Registration.Application.Queries.ListMarketRegistrations;
using RegOS.Registration.Application.Queries.ListProductRegistrations;
using RegOS.Registration.Application.Queries.ListRegistrationMarkets;
using RegOS.Registration.Application.Tests.Fixtures;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.Registration.Infrastructure.Repositories;
using RegOS.Registration.Infrastructure.Services;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;

using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;
using RegistrationAggregate = RegOS.Registration.Domain.Aggregates.Registration.Registration;
using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.Registration.Application.Tests;

/// <summary>
/// Integration tests — registrations against the real seeded reference data in
/// the dev Postgres.
/// </summary>
public sealed class RegistrationTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private static readonly CountryId UnitedStates =
        new(Guid.Parse("10000000-0000-0000-0000-000000000001"));
    private static readonly AuthorityId Fda =
        new(Guid.Parse("20000000-0000-0000-0000-000000000001"));

    /// <summary>Demo Manufacturer Ltd. — the tenant's own organization.</summary>
    private static readonly OrganizationId Holder =
        new(Guid.Parse("30000000-0000-0000-0000-000000000001"));

    private static readonly DateOnly Today = new(2026, 7, 31);

    private readonly List<Guid> _registrationIds = [];
    private readonly List<Guid> _productIds = [];
    private readonly List<Guid> _applicationIds = [];
    private readonly List<Guid> _organizationIds = [];

    private static RegOSDbContext New() =>
        new(
            new DbContextOptionsBuilder<RegOSDbContext>()
                .UseNpgsql(ConnectionString)
                .Options,
            TestTenant.Context);

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var ctx = New();

        foreach (var id in _registrationIds)
        {
            var registration = await ctx.Registrations
                .Include(x => x.History)
                .FirstOrDefaultAsync(x => x.Id == new RegistrationId(id));

            if (registration is not null)
                ctx.Registrations.Remove(registration);
        }

        await ctx.SaveChangesAsync();

        foreach (var id in _applicationIds)
        {
            var application = await ctx.RegulatoryApplications
                .FirstOrDefaultAsync(x => x.Id == new RegulatoryApplicationId(id));

            if (application is not null)
                ctx.RegulatoryApplications.Remove(application);
        }

        foreach (var id in _productIds)
        {
            var product = await ctx.Products
                .FirstOrDefaultAsync(x => x.Id == new GlobalProductId(id));

            if (product is not null)
                ctx.Products.Remove(product);
        }

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
    public async Task ARegistrationIsCreatedPlannedAndPersistedWithItsHistory()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);

        var id = await CreateAsync(ctx, globalProductId);

        await using var check = New();
        var registration = await check.Registrations
            .AsNoTracking()
            .Include(x => x.History)
            .FirstAsync(x => x.Id == id);

        registration.CurrentStatus.Should().Be(RegistrationStatus.Planned);
        registration.TenantId.Should().Be(TestTenant.Id);
        registration.History.Should().ContainSingle()
            .Which.OccurredOn.Should().Be(Today);
    }

    /// <summary>
    /// The constraint this epic deliberately does not impose. Real portfolios
    /// hold several authorisations in one market — different strengths,
    /// presentations, or holders after a partial divestment.
    /// </summary>
    [Fact]
    public async Task AProductMayHoldSeveralRegistrationsInTheSameMarket()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);

        await CreateAsync(ctx, globalProductId);
        var second = async () => await CreateAsync(ctx, globalProductId);

        await second.Should().NotThrowAsync();

        await using var check = New();
        var count = await check.Registrations
            .AsNoTracking()
            .CountAsync(x => x.GlobalProductId == globalProductId);

        count.Should().Be(2);
    }

    [Fact]
    public async Task AnUnknownProductIsNotFound()
    {
        await using var ctx = New();

        var create = async () => await CreateAsync(ctx, GlobalProductId.New());

        await create.Should().ThrowAsync<NotFoundException>()
            .WithMessage(RegistrationRuleErrors.ProductDoesNotExist);
    }

    [Fact]
    public async Task TheAuthorityMustBelongToTheCountry()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);

        // A country the FDA does not belong to.
        var elsewhere = await ctx.Countries
            .AsNoTracking()
            .Where(x => x.Id != UnitedStates)
            .Select(x => x.Id)
            .FirstAsync();

        var create = async () => await CreateAsync(ctx, globalProductId, countryId: elsewhere);

        await create.Should().ThrowAsync<DomainException>()
            .WithMessage(RegistrationRuleErrors.AuthorityNotInCountry);
    }

    [Fact]
    public async Task TheHolderMustBeActive()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);

        var retired = OrganizationAggregate.Create(
            TestTenant.Id, $"Retired Holder {Guid.NewGuid()}", OrganizationType.Manufacturer);
        retired.Deactivate();

        ctx.Organizations.Add(retired);
        await ctx.SaveChangesAsync();
        _organizationIds.Add(retired.Id.Value);

        var create = async () =>
            await CreateAsync(ctx, globalProductId, holderId: retired.Id);

        await create.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(RegistrationRuleErrors.OrganizationInactive);
    }

    // --- Provenance ----------------------------------------------------------

    [Fact]
    public async Task AnAuthorisationFiledElsewhereNeedsNoApplication()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);

        var id = await CreateAsync(ctx, globalProductId);

        await using var check = New();
        var registration = await check.Registrations
            .AsNoTracking().FirstAsync(x => x.Id == id);

        registration.OriginatingApplicationId.Should().BeNull();
    }

    [Fact]
    public async Task TheOriginatingApplicationIsRecordedWhenNamed()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        var applicationId = await ApplicationAsync(ctx, globalProductId);

        var id = await CreateAsync(ctx, globalProductId, applicationId: applicationId);

        await using var check = New();
        var registration = await check.Registrations
            .AsNoTracking().FirstAsync(x => x.Id == id);

        registration.OriginatingApplicationId.Should().Be(applicationId);
    }

    [Fact]
    public async Task AnApplicationForAnotherProductIsRejected()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        var otherProductId = await ProductAsync(ctx);
        var foreignApplication = await ApplicationAsync(ctx, otherProductId);

        var create = async () =>
            await CreateAsync(ctx, globalProductId, applicationId: foreignApplication);

        await create.Should().ThrowAsync<DomainException>()
            .WithMessage(RegistrationRuleErrors.ApplicationNotForProduct);
    }

    // --- Recording the grant -------------------------------------------------

    [Fact]
    public async Task RecordingApprovalPersistsTheNumberDatesAndASecondHistoryEntry()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);

        // A migrated authorisation: both entries carry their 2019 business
        // dates, in the order they happened, while both are recorded today.
        var id = await CreateAsync(
            ctx, globalProductId, occurredOn: new DateOnly(2019, 1, 15));

        var granted = new DateOnly(2019, 4, 12);

        await using var act = New();
        await new RecordRegistrationApprovalHandler(new RegistrationRepository(act))
            .HandleAsync(
                new RecordRegistrationApprovalCommand(
                    id, "NDA-123456", granted, new DateOnly(2029, 4, 12),
                    "Carried over from the legacy register."),
                default);

        await using var check = New();
        var registration = await check.Registrations
            .AsNoTracking()
            .Include(x => x.History)
            .FirstAsync(x => x.Id == id);

        registration.CurrentStatus.Should().Be(RegistrationStatus.Approved);
        registration.RegistrationNumber.Should().Be("NDA-123456");
        registration.ApprovedOn.Should().Be(granted);
        registration.History.Should().HaveCount(2);

        var approval = registration.History
            .Single(h => h.Status == RegistrationStatus.Approved);

        approval.OccurredOn.Should().Be(granted);
        approval.RecordedOnUtc.Date.Should().Be(DateTime.UtcNow.Date);
    }

    [Fact]
    public async Task RecordingApprovalOnAMissingRegistrationIsNotFound()
    {
        await using var ctx = New();

        var record = async () =>
            await new RecordRegistrationApprovalHandler(new RegistrationRepository(ctx))
                .HandleAsync(
                    new RecordRegistrationApprovalCommand(
                        RegistrationId.New(), "NDA-1", Today),
                    default);

        await record.Should().ThrowAsync<NotFoundException>()
            .WithMessage(RegistrationRuleErrors.RegistrationDoesNotExist);
    }

    // --- Lifecycle -----------------------------------------------------------

    /// <summary>
    /// The whole regulatory story of an authorisation, round-tripped through
    /// Postgres: filed, assessed, granted, suspended, reinstated, surrendered.
    /// The history is the record a regulator would read.
    /// </summary>
    [Fact]
    public async Task AnEntireLifecycleIsPersistedAsOneChronologicalHistory()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        var id = await CreateAsync(
            ctx, globalProductId, occurredOn: new DateOnly(2020, 1, 10));

        await ChangeAsync(id, RegistrationStatus.Submitted, new(2020, 3, 2));
        await ChangeAsync(id, RegistrationStatus.UnderReview, new(2020, 4, 15));
        await ApproveAsync(id, "NDA-556677", new(2021, 2, 8), new(2031, 2, 8));
        await ChangeAsync(
            id, RegistrationStatus.Suspended, new(2023, 9, 14),
            "GMP non-compliance at the manufacturing site.");
        await ChangeAsync(
            id, RegistrationStatus.Approved, new(2024, 1, 30),
            "Suspension lifted.");
        await ChangeAsync(id, RegistrationStatus.Withdrawn, new(2025, 6, 1));

        await using var check = New();
        var registration = await check.Registrations
            .AsNoTracking()
            .Include(x => x.History)
            .FirstAsync(x => x.Id == id);

        registration.CurrentStatus.Should().Be(RegistrationStatus.Withdrawn);

        registration.History
            .OrderBy(entry => entry.OccurredOn)
            .Select(entry => entry.Status)
            .Should().Equal(
                RegistrationStatus.Planned,
                RegistrationStatus.Submitted,
                RegistrationStatus.UnderReview,
                RegistrationStatus.Approved,
                RegistrationStatus.Suspended,
                RegistrationStatus.Approved,
                RegistrationStatus.Withdrawn);

        // The grant survives everything that happened after it.
        registration.RegistrationNumber.Should().Be("NDA-556677");
        registration.ApprovedOn.Should().Be(new DateOnly(2021, 2, 8));
    }

    [Fact]
    public async Task AForbiddenTransitionIsRefusedAndNothingIsWritten()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        var id = await CreateAsync(ctx, globalProductId);

        var suspend = async () =>
            await ChangeAsync(id, RegistrationStatus.Suspended, Today);

        await suspend.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(RegistrationErrors.TransitionNotPermitted(
                RegistrationStatus.Planned, RegistrationStatus.Suspended));

        await using var check = New();
        var registration = await check.Registrations
            .AsNoTracking()
            .Include(x => x.History)
            .FirstAsync(x => x.Id == id);

        registration.CurrentStatus.Should().Be(RegistrationStatus.Planned);
        registration.History.Should().ContainSingle();
    }

    [Fact]
    public async Task ATerminalRegistrationStaysWhereItIs()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        var id = await CreateAsync(ctx, globalProductId);

        await ChangeAsync(id, RegistrationStatus.Refused, Today);

        var revive = async () =>
            await ApproveAsync(id, "NDA-1", Today.AddDays(1));

        await revive.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(RegistrationErrors.StatusIsTerminal(
                RegistrationStatus.Refused));
    }

    [Fact]
    public async Task ChangingStatusOnAMissingRegistrationIsNotFound()
    {
        var change = async () => await ChangeAsync(
            RegistrationId.New(), RegistrationStatus.Submitted, Today);

        await change.Should().ThrowAsync<NotFoundException>()
            .WithMessage(RegistrationRuleErrors.RegistrationDoesNotExist);
    }

    // --- Reading -------------------------------------------------------------

    [Fact]
    public async Task TheDetailViewResolvesTheNamesAPersonReads()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        var id = await CreateAsync(ctx, globalProductId);

        await using var check = New();
        var detail = await new GetRegistrationHandler(check).HandleAsync(id, default);

        detail.Should().NotBeNull();
        detail!.CountryName.Should().NotBeNullOrWhiteSpace();
        detail.AuthorityName.Should().NotBeNullOrWhiteSpace();
        detail.HolderOrganizationName.Should().NotBeNullOrWhiteSpace();
        detail.ProductName.Should().NotBeNullOrWhiteSpace();
        detail.Status.Should().Be(nameof(RegistrationStatus.Planned));
        detail.History.Should().ContainSingle();
    }

    /// <summary>
    /// The read model asks the domain where a registration may go, so a client
    /// offers exactly the choices the domain would accept instead of restating
    /// the rules — and a terminal registration offers none.
    /// </summary>
    [Fact]
    public async Task TheDetailViewOffersTheTransitionsTheDomainWouldAccept()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        var id = await CreateAsync(ctx, globalProductId);

        await using var planned = New();
        var whilePlanned = await new GetRegistrationHandler(planned)
            .HandleAsync(id, default);

        whilePlanned!.AllowedNextStatuses.Should().BeEquivalentTo(
        [
            nameof(RegistrationStatus.Submitted),
            nameof(RegistrationStatus.UnderReview),
            nameof(RegistrationStatus.Approved),
            nameof(RegistrationStatus.Refused),
            nameof(RegistrationStatus.Withdrawn),
        ]);

        await ChangeAsync(id, RegistrationStatus.Refused, Today);

        await using var refused = New();
        var whenRefused = await new GetRegistrationHandler(refused)
            .HandleAsync(id, default);

        whenRefused!.AllowedNextStatuses.Should().BeEmpty();
    }

    [Fact]
    public async Task AMissingRegistrationHasNoDetail()
    {
        await using var ctx = New();

        var detail = await new GetRegistrationHandler(ctx)
            .HandleAsync(RegistrationId.New(), default);

        detail.Should().BeNull();
    }

    [Fact]
    public async Task TheProductPortfolioListsWhatItHolds()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        await CreateAsync(ctx, globalProductId);

        await using var check = New();
        var rows = await new ListProductRegistrationsHandler(check)
            .HandleAsync(globalProductId, default);

        rows.Should().NotBeNull();
        var row = rows!.Should().ContainSingle().Subject;

        row.CountryName.Should().NotBeNullOrWhiteSpace();
        row.Status.Should().Be(nameof(RegistrationStatus.Planned));
        row.RegistrationNumber.Should().BeNull("nothing has been granted yet");
    }

    [Fact]
    public async Task AnUnknownProductHasNoPortfolio()
    {
        await using var ctx = New();

        var rows = await new ListProductRegistrationsHandler(ctx)
            .HandleAsync(GlobalProductId.New(), default);

        rows.Should().BeNull();
    }

    // --- The market axis -----------------------------------------------------

    [Fact]
    public async Task TheMarketViewListsWhatIsHeldThereByProduct()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        var id = await CreateAsync(ctx, globalProductId);

        await using var check = New();
        var rows = await new ListMarketRegistrationsHandler(check)
            .HandleAsync(UnitedStates, default);

        var row = rows!.Should().ContainSingle(x => x.RegistrationId == id.Value)
            .Subject;

        row.GlobalProductId.Should().Be(globalProductId.Value);
        row.ProductName.Should().NotBeNullOrWhiteSpace();
        row.ProductCode.Should().NotBeNullOrWhiteSpace();
        row.AuthorityName.Should().NotBeNullOrWhiteSpace();
        row.Status.Should().Be(nameof(RegistrationStatus.Planned));
    }

    /// <summary>
    /// "What do we hold" is not "what is currently marketable": a withdrawn
    /// authorisation is still part of the regulatory portfolio, so nothing is
    /// filtered away. Narrowing the view is the client's business.
    /// </summary>
    [Fact]
    public async Task TheMarketViewHidesNothingButLeadsWithLiveAuthorisations()
    {
        await using var ctx = New();

        var surrendered = await ProductAsync(ctx);
        var surrenderedId = await CreateAsync(
            ctx, surrendered, occurredOn: new DateOnly(2020, 1, 1));
        await ChangeAsync(
            surrenderedId, RegistrationStatus.Withdrawn, new(2021, 1, 1));

        var live = await ProductAsync(ctx);
        var liveId = await CreateAsync(
            ctx, live, occurredOn: new DateOnly(2020, 1, 1));
        await ApproveAsync(liveId, "NDA-live", new(2021, 6, 1));

        await using var check = New();
        var rows = await new ListMarketRegistrationsHandler(check)
            .HandleAsync(UnitedStates, default);

        var mine = rows!
            .Where(x => x.RegistrationId == liveId.Value
                || x.RegistrationId == surrenderedId.Value)
            .ToList();

        mine.Should().HaveCount(2, "nothing is filtered away");
        mine[0].RegistrationId.Should().Be(liveId.Value, "approved leads");
        mine[1].RegistrationId.Should().Be(surrenderedId.Value);
    }

    [Fact]
    public async Task AnUnknownCountryHasNoMarketView()
    {
        await using var ctx = New();

        var rows = await new ListMarketRegistrationsHandler(ctx)
            .HandleAsync(new CountryId(Guid.NewGuid()), default);

        rows.Should().BeNull();
    }

    /// <summary>
    /// The way into the market view: only countries something is actually held
    /// in, because nobody starts by browsing two hundred of them.
    /// </summary>
    [Fact]
    public async Task TheMarketsIndexNamesOnlyCountriesSomethingIsHeldIn()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        await CreateAsync(ctx, globalProductId);
        await CreateAsync(ctx, globalProductId);

        await using var check = New();
        var markets = await new ListRegistrationMarketsHandler(check)
            .HandleAsync(default);

        var market = markets.Should()
            .ContainSingle(x => x.CountryId == UnitedStates.Value).Subject;

        market.CountryName.Should().NotBeNullOrWhiteSpace();
        market.CountryCode.Should().NotBeNullOrWhiteSpace();
        market.RegistrationCount.Should().BeGreaterThanOrEqualTo(2);

        // A country with nothing in it is simply absent — this is navigation,
        // not a catalogue, so the index is far shorter than the country list.
        var countries = await check.Countries.AsNoTracking().CountAsync();

        markets.Should().OnlyContain(x => x.RegistrationCount > 0);
        markets.Count.Should().BeLessThan(countries);
    }

    // --- Attention -----------------------------------------------------------

    /// <summary>
    /// The third portfolio question — the only one that spans every market at
    /// once. The set is objective: whose validity is still running, nearest
    /// first. What counts as urgent is nobody's business here.
    /// </summary>
    [Fact]
    public async Task TheExpiringListLeadsWithWhateverEndsSoonest()
    {
        await using var ctx = New();

        var later = await GrantedAsync(ctx, "NDA-later", new DateOnly(2030, 1, 1));
        var sooner = await GrantedAsync(ctx, "NDA-sooner", new DateOnly(2027, 1, 1));

        await using var check = New();
        var rows = await new ListExpiringRegistrationsHandler(check)
            .HandleAsync(default);

        var mine = rows
            .Where(x => x.RegistrationId == later.Value
                || x.RegistrationId == sooner.Value)
            .ToList();

        mine.Should().HaveCount(2);
        mine[0].RegistrationId.Should().Be(sooner.Value);
        mine[1].RegistrationId.Should().Be(later.Value);

        mine[0].DaysUntilExpiry.Should().BeLessThan(mine[1].DaysUntilExpiry);
        mine[0].ProductName.Should().NotBeNullOrWhiteSpace();
        mine[0].CountryName.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// A surrendered authorisation keeps the expiry date it was granted with,
    /// but it has left the validity timeline — so it is not something to act on.
    /// </summary>
    [Fact]
    public async Task ASurrenderedRegistrationLeavesTheExpiringList()
    {
        await using var ctx = New();
        var id = await GrantedAsync(ctx, "NDA-gone", new DateOnly(2030, 1, 1));

        await using var before = New();
        var listed = await new ListExpiringRegistrationsHandler(before)
            .HandleAsync(default);
        listed.Should().Contain(x => x.RegistrationId == id.Value);

        await ChangeAsync(id, RegistrationStatus.Withdrawn, new(2026, 6, 1));

        await using var after = New();
        var remaining = await new ListExpiringRegistrationsHandler(after)
            .HandleAsync(default);

        remaining.Should().NotContain(x => x.RegistrationId == id.Value);

        // The date itself is untouched — it is history, not a countdown.
        await using var check = New();
        var detail = await new GetRegistrationHandler(check)
            .HandleAsync(id, default);

        detail!.ExpiresOn.Should().Be(new DateOnly(2030, 1, 1));
        detail.HasRunningValidity.Should().BeFalse();
        detail.DaysUntilExpiry.Should().BeNull();
    }

    /// <summary>
    /// Lapsed in the world, not yet recorded here — the strongest signal the
    /// portfolio has, and it leads the list rather than being clamped away.
    /// </summary>
    [Fact]
    public async Task AnAuthorisationPastItsExpiryReportsHowLongAgo()
    {
        await using var ctx = New();
        var id = await GrantedAsync(ctx, "NDA-lapsed", new DateOnly(2021, 1, 1));

        await using var check = New();
        var rows = await new ListExpiringRegistrationsHandler(check)
            .HandleAsync(default);

        var row = rows.Should().ContainSingle(x => x.RegistrationId == id.Value)
            .Subject;

        row.DaysUntilExpiry.Should().BeNegative();
        row.IsExpired.Should().BeTrue();

        // Nearest first means the overdue ones come before anything future.
        rows[0].DaysUntilExpiry.Should().BeLessThanOrEqualTo(
            rows[^1].DaysUntilExpiry);
    }

    [Fact]
    public async Task ARegistrationWithNoExpiryDateIsNotSomethingToActOn()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        var id = await CreateAsync(
            ctx, globalProductId, occurredOn: new DateOnly(2020, 1, 1));

        // Granted with no validity period — real for authorisations that do not
        // lapse.
        await ApproveAsync(id, "NDA-perpetual", new DateOnly(2021, 1, 1));

        await using var check = New();
        var rows = await new ListExpiringRegistrationsHandler(check)
            .HandleAsync(default);

        rows.Should().NotContain(x => x.RegistrationId == id.Value);

        var detail = await new GetRegistrationHandler(check)
            .HandleAsync(id, default);

        // Still on the timeline — it simply has no date to count towards.
        detail!.HasRunningValidity.Should().BeTrue();
        detail.DaysUntilExpiry.Should().BeNull();
    }

    [Fact]
    public async Task ThePortfolioViewsCarryTheSameDerivedExpiry()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        var id = await CreateAsync(
            ctx, globalProductId, occurredOn: new DateOnly(2020, 1, 1));
        await ApproveAsync(
            id, "NDA-both", new DateOnly(2021, 1, 1), new DateOnly(2030, 1, 1));

        await using var check = New();

        var byProduct = (await new ListProductRegistrationsHandler(check)
            .HandleAsync(globalProductId, default))!.Single();

        var byMarket = (await new ListMarketRegistrationsHandler(check)
            .HandleAsync(UnitedStates, default))!
            .Single(x => x.RegistrationId == id.Value);

        byProduct.DaysUntilExpiry.Should().Be(byMarket.DaysUntilExpiry);
        byProduct.DaysUntilExpiry.Should().BePositive();
        byProduct.HasRunningValidity.Should().BeTrue();
        byProduct.IsExpired.Should().BeFalse();
    }

    // --- helpers -------------------------------------------------------------

    /// <summary>A granted registration in the US market, with a validity period.</summary>
    private async Task<RegistrationId> GrantedAsync(
        RegOSDbContext ctx,
        string registrationNumber,
        DateOnly expiresOn)
    {
        var globalProductId = await ProductAsync(ctx);
        var id = await CreateAsync(
            ctx, globalProductId, occurredOn: new DateOnly(2020, 1, 1));

        await ApproveAsync(id, registrationNumber, new DateOnly(2021, 1, 1), expiresOn);

        return id;
    }

    private async Task<RegistrationId> CreateAsync(
        RegOSDbContext ctx,
        GlobalProductId globalProductId,
        CountryId? countryId = null,
        OrganizationId? holderId = null,
        RegulatoryApplicationId? applicationId = null,
        DateOnly? occurredOn = null)
    {
        var handler = new CreateRegistrationHandler(
            new RegistrationCreationPolicy(ctx),
            new RegistrationRepository(ctx),
            TestTenant.Context);

        var result = await handler.HandleAsync(
            new CreateRegistrationCommand(
                globalProductId,
                countryId ?? UnitedStates,
                Fda,
                holderId ?? Holder,
                occurredOn ?? Today,
                applicationId),
            default);

        _registrationIds.Add(result.Id.Value);

        return result.Id;
    }

    /// <summary>
    /// Each transition through its own context, the way a request would arrive —
    /// so the lifecycle is exercised against persisted state rather than an
    /// aggregate that never left memory.
    /// </summary>
    private static async Task ChangeAsync(
        RegistrationId id,
        RegistrationStatus status,
        DateOnly occurredOn,
        string? note = null)
    {
        await using var ctx = New();

        await new ChangeRegistrationStatusHandler(new RegistrationRepository(ctx))
            .HandleAsync(
                new ChangeRegistrationStatusCommand(id, status, occurredOn, note),
                default);
    }

    private static async Task ApproveAsync(
        RegistrationId id,
        string registrationNumber,
        DateOnly approvedOn,
        DateOnly? expiresOn = null)
    {
        await using var ctx = New();

        await new RecordRegistrationApprovalHandler(new RegistrationRepository(ctx))
            .HandleAsync(
                new RecordRegistrationApprovalCommand(
                    id, registrationNumber, approvedOn, expiresOn),
                default);
    }

    private async Task<GlobalProductId> ProductAsync(RegOSDbContext ctx)
    {
        var product = GlobalProduct.Register(
            TestTenant.Id,
            $"REG-{Guid.NewGuid():N}"[..20],
            "Registration Test Product",
            ProductType.Drug);

        ctx.Products.Add(product);
        await ctx.SaveChangesAsync();
        _productIds.Add(product.Id.Value);

        return product.Id;
    }

    private async Task<RegulatoryApplicationId> ApplicationAsync(
        RegOSDbContext ctx, GlobalProductId globalProductId)
    {
        var application = RegulatoryApplicationAggregate.Create(
            TestTenant.Id, globalProductId, UnitedStates, Fda, Holder,
            "Registration Test Application");

        ctx.RegulatoryApplications.Add(application);
        await ctx.SaveChangesAsync();
        _applicationIds.Add(application.Id.Value);

        return application.Id;
    }
}
