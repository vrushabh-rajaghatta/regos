using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.Product.Application.Commands.ActivateMedicinalProduct;
using RegOS.Product.Application.Commands.AddTradeName;
using RegOS.Product.Application.Commands.ChangeMarketStatus;
using RegOS.Product.Application.Commands.CreateMedicinalProduct;
using RegOS.Product.Application.Commands.DeactivateMedicinalProduct;
using RegOS.Product.Application.Commands.RemoveTradeName;
using RegOS.Product.Application.Queries.ListMedicinalProducts;
using RegOS.Product.Application.Services;
using RegOS.Product.Application.Tests.Fixtures;
using RegOS.Product.Domain.Product;
using RegOS.Product.Infrastructure.Persistence;
using RegOS.Product.Infrastructure.Services;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Registration.Application.Commands.CreateRegistration;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.Registration.Infrastructure.Repositories;
using RegOS.Registration.Infrastructure.Services;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Tests;

/// <summary>
/// The market-local tier, exercised through its handlers against the real
/// seeded reference data in the dev Postgres.
/// </summary>
/// <remarks>
/// Through the handlers, never the aggregate: EPIC-016 shipped domain
/// behaviour twice that no caller could reach, and these tests are written so
/// that could not happen here.
/// </remarks>
public sealed class MedicinalProductTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private static readonly CountryId UnitedStates =
        new(Guid.Parse("10000000-0000-0000-0000-000000000001"));

    private static readonly DateOnly Today = new(2026, 7, 31);

    /// <summary>A market entered long ago, so its history has room to run.</summary>
    private static readonly DateOnly Entered = new(2020, 1, 1);

    /// <summary>The seeded FDA and Demo Manufacturer Ltd.</summary>
    private static readonly AuthorityId Fda =
        new(Guid.Parse("20000000-0000-0000-0000-000000000001"));
    private static readonly OrganizationId Holder =
        new(Guid.Parse("30000000-0000-0000-0000-000000000001"));

    private readonly List<Guid> _registrationIds = [];
    private readonly List<Guid> _medicinalProductIds = [];
    private readonly List<Guid> _productIds = [];

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

        // Registrations first: they point at the markets removed below, and
        // that FK is Restrict.
        foreach (var id in _registrationIds)
        {
            var registration = await ctx.Registrations
                .Include(x => x.History)
                .FirstOrDefaultAsync(x => x.Id == new RegistrationId(id));

            if (registration is not null)
                ctx.Registrations.Remove(registration);
        }

        await ctx.SaveChangesAsync();

        foreach (var id in _medicinalProductIds)
        {
            var market = await ctx.MedicinalProducts
                .FirstOrDefaultAsync(x => x.Id == new MedicinalProductId(id));

            if (market is not null)
                ctx.MedicinalProducts.Remove(market);
        }

        await ctx.SaveChangesAsync();

        foreach (var id in _productIds)
        {
            var product = await ctx.Products
                .FirstOrDefaultAsync(x => x.Id == new GlobalProductId(id));

            if (product is not null)
                ctx.Products.Remove(product);
        }

        await ctx.SaveChangesAsync();
    }

    // --- Creating ------------------------------------------------------------

    [Fact]
    public async Task AMarketPresenceIsCreatedActiveAndPersisted()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);

        var id = await CreateAsync(ctx, globalProductId);

        await using var check = New();
        var market = await check.MedicinalProducts
            .AsNoTracking()
            .FirstAsync(x => x.Id == id);

        market.GlobalProductId.Should().Be(globalProductId);
        market.CountryId.Should().Be(UnitedStates);
        market.Status.Should().Be(MedicinalProductStatus.Active);
        market.StatusDate.Should().Be(Today);
        market.TenantId.Should().Be(TestTenant.Id);
    }

    /// <summary>
    /// <b>The constraint this epic deliberately does not impose</b>, one tier
    /// above the EPIC-005 one. Presentations, strengths and the two halves of a
    /// partial divestment are all several medicinal products in one market —
    /// and it is this absence that makes resolve-or-create impossible rather
    /// than merely unwise.
    /// </summary>
    [Fact]
    public async Task AProductMayHaveSeveralMarketPresencesInOneCountry()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);

        var first = await CreateAsync(ctx, globalProductId);
        var second = await CreateAsync(ctx, globalProductId);

        second.Should().NotBe(first);

        await using var check = New();
        var count = await check.MedicinalProducts
            .AsNoTracking()
            .CountAsync(x => x.GlobalProductId == globalProductId
                && x.CountryId == UnitedStates);

        count.Should().Be(2);
    }

    [Fact]
    public async Task AnUnknownProductIsNotFound()
    {
        await using var ctx = New();

        var create = async () => await CreateAsync(ctx, GlobalProductId.New());

        await create.Should().ThrowAsync<NotFoundException>()
            .WithMessage(MedicinalProductPolicyErrors.GlobalProductDoesNotExist);
    }

    [Fact]
    public async Task AnUnknownCountryIsRejected()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);

        var create = async () => await CreateAsync(
            ctx, globalProductId, new CountryId(Guid.NewGuid()));

        await create.Should().ThrowAsync<DomainException>()
            .WithMessage(MedicinalProductPolicyErrors.CountryDoesNotExist);
    }

    /// <summary>
    /// A market presence exists from the moment a company intends to market
    /// there — years before any authorisation. Nothing about creating one
    /// mentions a registration.
    /// </summary>
    [Fact]
    public async Task AMarketPresenceNeedsNoRegistration()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        var id = await CreateAsync(ctx, globalProductId);

        await using var check = New();
        var held = await check.Registrations
            .AsNoTracking()
            .CountAsync(x => x.MedicinalProductId == id);

        held.Should().Be(0);
    }

    // --- Trade names ---------------------------------------------------------

    [Fact]
    public async Task ATradeNameIsRecordedAgainstTheMarket()
    {
        await using var ctx = New();
        var market = await CreateAsync(ctx, await ProductAsync(ctx));

        await AddTradeNameAsync(market, "en", "Cardiolex");

        await using var check = New();
        var reloaded = await check.MedicinalProducts
            .AsNoTracking()
            .Include(x => x.TradeNames)
            .FirstAsync(x => x.Id == market);

        var tradeName = reloaded.TradeNames.Should().ContainSingle().Subject;

        tradeName.Name.Should().Be("Cardiolex");
        tradeName.Language.Should().Be(LanguageCode.Parse("en"));
    }

    /// <summary>
    /// The deliberate opposite of the rule one tier up. Two market presences in
    /// one country are two business objects; two English names for one market
    /// presence are two labels for one thing, so one of them is wrong.
    /// </summary>
    [Fact]
    public async Task AMarketMayHaveOneNamePerLanguageAndNoMore()
    {
        await using var ctx = New();
        var market = await CreateAsync(ctx, await ProductAsync(ctx));

        await AddTradeNameAsync(market, "en", "Cardiolex");
        await AddTradeNameAsync(market, "fr", "Cardiolexe");

        var third = async () => await AddTradeNameAsync(market, "en", "Cardio");

        await third.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(MedicinalProductErrors.TradeNameLanguageAlreadyRecorded);
    }

    /// <summary>
    /// <b>The test that matters.</b> Proving the aggregate rejects a duplicate
    /// in memory is easy; proving it still rejects one after a save and a
    /// reload is what validates the <c>Include</c>, the repository and the
    /// handler as a single slice. EPIC-016 learned this the hard way.
    /// </summary>
    [Fact]
    public async Task TheOneNamePerLanguageRuleSurvivesAReload()
    {
        await using var ctx = New();
        var market = await CreateAsync(ctx, await ProductAsync(ctx));

        await AddTradeNameAsync(market, "en", "Cardiolex");

        // A fresh context, exactly as a second request would arrive.
        var again = async () => await AddTradeNameAsync(market, "en", "Other");

        await again.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(MedicinalProductErrors.TradeNameLanguageAlreadyRecorded);
    }

    [Fact]
    public async Task CaseIsNotASecondLanguage()
    {
        await using var ctx = New();
        var market = await CreateAsync(ctx, await ProductAsync(ctx));

        await AddTradeNameAsync(market, "en", "Cardiolex");

        var shouting = async () => await AddTradeNameAsync(market, "EN", "Other");

        await shouting.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(MedicinalProductErrors.TradeNameLanguageAlreadyRecorded);
    }

    /// <summary>
    /// Removing frees the language again — which is also the only way to
    /// correct a name, and why there is no Rename.
    /// </summary>
    [Fact]
    public async Task RemovingANameFreesItsLanguage()
    {
        await using var ctx = New();
        var market = await CreateAsync(ctx, await ProductAsync(ctx));
        var id = await AddTradeNameAsync(market, "en", "Cardiolex");

        await using var removing = New();
        await new RemoveTradeNameHandler(new MedicinalProductRepository(removing))
            .HandleAsync(new RemoveTradeNameCommand(market, id), default);

        var readded = async () => await AddTradeNameAsync(market, "en", "Renamed");

        await readded.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemovingANameThatIsNotThereIsNotFound()
    {
        await using var ctx = New();
        var market = await CreateAsync(ctx, await ProductAsync(ctx));

        var remove = async () =>
            await new RemoveTradeNameHandler(new MedicinalProductRepository(ctx))
                .HandleAsync(
                    new RemoveTradeNameCommand(market, TradeNameId.New()),
                    default);

        await remove.Should().ThrowAsync<NotFoundException>()
            .WithMessage(MedicinalProductErrors.TradeNameNotFound);
    }

    [Fact]
    public async Task AddingANameToAMarketThatIsNotThereIsNotFound()
    {
        var add = async () =>
            await AddTradeNameAsync(MedicinalProductId.New(), "en", "Cardiolex");

        await add.Should().ThrowAsync<NotFoundException>()
            .WithMessage(
                MedicinalProductPolicyErrors.MedicinalProductDoesNotExist);
    }

    [Fact]
    public async Task AMalformedLanguageIsRejectedBeforeAnythingIsLoaded()
    {
        await using var ctx = New();
        var market = await CreateAsync(ctx, await ProductAsync(ctx));

        var add = async () => await AddTradeNameAsync(market, "en-CA", "Cardiolex");

        await add.Should().ThrowAsync<DomainException>()
            .WithMessage(MedicinalProductErrors.LanguageNotRecognised);
    }

    // --- Market status -------------------------------------------------------

    [Fact]
    public async Task AMarketIsCreatedPlannedAndPersistedWithItsHistory()
    {
        await using var ctx = New();
        var id = await CreateAsync(ctx, await ProductAsync(ctx));

        await using var check = New();
        var market = await check.MedicinalProducts
            .AsNoTracking()
            .Include(x => x.MarketStatusHistory)
            .FirstAsync(x => x.Id == id);

        market.CurrentMarketStatus.Should().Be(MarketStatus.Planned);
        market.MarketStatusHistory.Should().ContainSingle()
            .Which.OccurredOn.Should().Be(Today);
    }

    /// <summary>
    /// The whole commercial story of a market, round-tripped through Postgres.
    /// The equivalent of EPIC-005's registration-lifecycle test, one tier down —
    /// and the sequence it proves is one a registration could never hold.
    /// </summary>
    [Fact]
    public async Task AnEntireCommercialLifeIsPersistedAsOneChronologicalHistory()
    {
        await using var ctx = New();
        var id = await CreateAsync(ctx, await ProductAsync(ctx), statusDate: Entered);

        await ChangeAsync(id, MarketStatus.Launched, new(2021, 3, 15));
        await ChangeAsync(
            id, MarketStatus.TemporarilyUnavailable, new(2023, 8, 1),
            "API supply interruption.");
        await ChangeAsync(id, MarketStatus.Launched, new(2024, 2, 1));
        await ChangeAsync(id, MarketStatus.Discontinued, new(2026, 1, 15));

        await using var check = New();
        var market = await check.MedicinalProducts
            .AsNoTracking()
            .Include(x => x.MarketStatusHistory)
            .FirstAsync(x => x.Id == id);

        market.CurrentMarketStatus.Should().Be(MarketStatus.Discontinued);

        market.MarketStatusHistory
            .OrderBy(entry => entry.OccurredOn)
            .Select(entry => entry.Status)
            .Should().Equal(
                MarketStatus.Planned,
                MarketStatus.Launched,
                MarketStatus.TemporarilyUnavailable,
                MarketStatus.Launched,
                MarketStatus.Discontinued);

        market.MarketStatusHistory
            .Should().ContainSingle(entry => entry.Note != null)
            .Which.Note.Should().Be("API supply interruption.");
    }

    /// <summary>
    /// The chronology rule survives a reload — the same proof the trade-name
    /// rule gets, because it also depends on the aggregate seeing its own
    /// history through the repository's Include.
    /// </summary>
    [Fact]
    public async Task TheChronologyRuleSurvivesAReload()
    {
        await using var ctx = New();
        var id = await CreateAsync(ctx, await ProductAsync(ctx), statusDate: Entered);

        await ChangeAsync(id, MarketStatus.Launched, new(2021, 3, 15));

        var backdated = async () =>
            await ChangeAsync(id, MarketStatus.Discontinued, new(2020, 1, 1));

        await backdated.Should().ThrowAsync<DomainException>()
            .WithMessage(MedicinalProductErrors.OccurredOnBeforePreviousEntry);
    }

    [Fact]
    public async Task AMarketAlreadyEnteredCannotBePlannedAgain()
    {
        await using var ctx = New();
        var id = await CreateAsync(ctx, await ProductAsync(ctx), statusDate: Entered);

        await ChangeAsync(id, MarketStatus.Launched, new(2021, 3, 15));

        var replan = async () =>
            await ChangeAsync(id, MarketStatus.Planned, new(2022, 1, 1));

        await replan.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(MedicinalProductErrors.MarketCannotBePlannedAgain);
    }

    [Fact]
    public async Task ChangingTheStatusOfAMarketThatIsNotThereIsNotFound()
    {
        var change = async () => await ChangeAsync(
            MedicinalProductId.New(), MarketStatus.Launched, Today);

        await change.Should().ThrowAsync<NotFoundException>()
            .WithMessage(
                MedicinalProductPolicyErrors.MedicinalProductDoesNotExist);
    }

    // --- Operability ---------------------------------------------------------

    /// <summary>
    /// <b>The rule this story deliberately does not impose.</b> Nothing
    /// consults the registrations held in this market, because "has
    /// registrations" is not the invariant anyone means — an expired one should
    /// not block it, nor a withdrawn one — and the rule would immediately
    /// become a policy over another aggregate's lifecycle, running against the
    /// dependency direction the whole epic established.
    /// </summary>
    [Fact]
    public async Task AMarketHoldingRegistrationsCanStillBeRetired()
    {
        await using var ctx = New();
        var market = await CreateAsync(ctx, await ProductAsync(ctx));
        var registrationId = await RegistrationAsync(ctx, market);

        var retire = async () =>
            await SetActivationAsync(market, active: false, on: Today);

        await retire.Should().NotThrowAsync();

        await using var check = New();

        var reloaded = await check.MedicinalProducts
            .AsNoTracking().FirstAsync(x => x.Id == market);

        reloaded.Status.Should().Be(MedicinalProductStatus.Inactive);
        reloaded.StatusDate.Should().Be(Today);

        // The licence is untouched, and still points at this market. Retiring
        // a record is not a regulatory event.
        var registration = await check.Registrations
            .AsNoTracking().FirstAsync(x => x.Id == registrationId);

        registration.MedicinalProductId.Should().Be(market);
        registration.CurrentStatus.Should().Be(RegistrationStatus.Planned);
    }

    /// <summary>
    /// Round-tripped through Postgres: retiring the record leaves the
    /// commercial history exactly where it was.
    /// </summary>
    [Fact]
    public async Task RetiringARecordLeavesItsCommercialHistoryUntouched()
    {
        await using var ctx = New();
        var market = await CreateAsync(
            ctx, await ProductAsync(ctx), statusDate: Entered);

        await ChangeAsync(market, MarketStatus.Launched, new(2021, 3, 15));
        await SetActivationAsync(market, active: false, on: new(2026, 4, 1));

        await using var check = New();
        var reloaded = await check.MedicinalProducts
            .AsNoTracking()
            .Include(x => x.MarketStatusHistory)
            .FirstAsync(x => x.Id == market);

        reloaded.Status.Should().Be(MedicinalProductStatus.Inactive);
        reloaded.CurrentMarketStatus.Should().Be(MarketStatus.Launched);
        reloaded.MarketStatusHistory.Should().HaveCount(2);

        // And back again, with the commercial state still untouched.
        await SetActivationAsync(market, active: true, on: new(2026, 5, 1));

        await using var after = New();
        var restored = await after.MedicinalProducts
            .AsNoTracking().FirstAsync(x => x.Id == market);

        restored.Status.Should().Be(MedicinalProductStatus.Active);
        restored.StatusDate.Should().Be(new DateOnly(2026, 5, 1));
        restored.CurrentMarketStatus.Should().Be(MarketStatus.Launched);
    }

    [Fact]
    public async Task RetiringARecordThatIsNotThereIsNotFound()
    {
        var retire = async () => await SetActivationAsync(
            MedicinalProductId.New(), active: false, on: Today);

        await retire.Should().ThrowAsync<NotFoundException>()
            .WithMessage(
                MedicinalProductPolicyErrors.MedicinalProductDoesNotExist);
    }

    // --- Reading -------------------------------------------------------------

    /// <summary>
    /// <b>The launch date is derived, never stored.</b> It cannot disagree with
    /// the history because it <em>is</em> the history — and a relaunch does not
    /// move it, because "when did we launch" means the original.
    /// </summary>
    [Fact]
    public async Task TheLaunchDateIsTheFirstLaunchAndARelaunchDoesNotMoveIt()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        var id = await CreateAsync(ctx, globalProductId, statusDate: Entered);

        await using var planned = New();
        var whilePlanned = await new ListMedicinalProductsHandler(planned)
            .HandleAsync(new ListMedicinalProductsQuery(globalProductId), default);

        whilePlanned!.Single().MarketStatus
            .Should().Be(nameof(MarketStatus.Planned));
        whilePlanned.Single().LaunchedOn
            .Should().BeNull("nothing has launched yet");

        await ChangeAsync(id, MarketStatus.Launched, new(2021, 3, 15));
        await ChangeAsync(id, MarketStatus.Discontinued, new(2023, 1, 1));
        await ChangeAsync(id, MarketStatus.Launched, new(2025, 6, 1));

        await using var check = New();
        var row = (await new ListMedicinalProductsHandler(check)
            .HandleAsync(new ListMedicinalProductsQuery(globalProductId), default))!
            .Single();

        row.MarketStatus.Should().Be(nameof(MarketStatus.Launched));
        row.LaunchedOn.Should().Be(new DateOnly(2021, 3, 15));
    }

    [Fact]
    public async Task TheMarketsOfAProductAreListedWithTheNamesAPersonReads()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        var id = await CreateAsync(ctx, globalProductId);

        await using var check = New();
        var rows = await new ListMedicinalProductsHandler(check)
            .HandleAsync(new ListMedicinalProductsQuery(globalProductId), default);

        rows.Should().NotBeNull();
        var row = rows!.Should().ContainSingle().Subject;

        row.MedicinalProductId.Should().Be(id.Value);
        row.CountryId.Should().Be(UnitedStates.Value);
        row.CountryName.Should().NotBeNullOrWhiteSpace();
        row.CountryCode.Should().NotBeNullOrWhiteSpace();
        row.Status.Should().Be(nameof(MedicinalProductStatus.Active));
        row.StatusDate.Should().Be(Today);
        row.TradeNames.Should().BeEmpty("branding is settled after entry");
    }

    [Fact]
    public async Task TheMarketsListCarriesWhatTheProductIsCalledThere()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);
        var market = await CreateAsync(ctx, globalProductId);

        await AddTradeNameAsync(market, "fr", "Cardiolexe");
        await AddTradeNameAsync(market, "en", "Cardiolex");

        await using var check = New();
        var rows = await new ListMedicinalProductsHandler(check)
            .HandleAsync(new ListMedicinalProductsQuery(globalProductId), default);

        var row = rows!.Should().ContainSingle().Subject;

        row.TradeNames.Select(name => name.Name)
            .Should().Equal("Cardiolex", "Cardiolexe");

        row.TradeNames.Select(name => name.Language)
            .Should().Equal("en", "fr");
    }

    /// <summary>
    /// Empty and missing are different answers: a product with no markets is
    /// ordinary, a product that never existed is a 404.
    /// </summary>
    [Fact]
    public async Task AProductInNoMarketListsNothingAndAnUnknownProductListsNull()
    {
        await using var ctx = New();
        var globalProductId = await ProductAsync(ctx);

        var none = await new ListMedicinalProductsHandler(ctx)
            .HandleAsync(new ListMedicinalProductsQuery(globalProductId), default);

        none.Should().NotBeNull().And.BeEmpty();

        var missing = await new ListMedicinalProductsHandler(ctx)
            .HandleAsync(
                new ListMedicinalProductsQuery(GlobalProductId.New()), default);

        missing.Should().BeNull();
    }

    // --- helpers -------------------------------------------------------------

    /// <summary>
    /// Each call through its own context, the way a request would arrive — so
    /// the one-name-per-language rule is exercised against persisted state
    /// rather than an aggregate that never left memory.
    /// </summary>
    private static async Task SetActivationAsync(
        MedicinalProductId market,
        bool active,
        DateOnly on)
    {
        await using var ctx = New();
        var repository = new MedicinalProductRepository(ctx);

        if (active)
        {
            await new ActivateMedicinalProductHandler(repository).HandleAsync(
                new ActivateMedicinalProductCommand(market, on), default);
        }
        else
        {
            await new DeactivateMedicinalProductHandler(repository).HandleAsync(
                new DeactivateMedicinalProductCommand(market, on), default);
        }
    }

    /// <summary>
    /// Each change through its own context, the way a request would arrive — so
    /// the chronology rule is exercised against persisted state rather than an
    /// aggregate that never left memory.
    /// </summary>
    private static async Task ChangeAsync(
        MedicinalProductId market,
        MarketStatus status,
        DateOnly occurredOn,
        string? note = null)
    {
        await using var ctx = New();

        await new ChangeMarketStatusHandler(new MedicinalProductRepository(ctx))
            .HandleAsync(
                new ChangeMarketStatusCommand(market, status, occurredOn, note),
                default);
    }

    private static async Task<TradeNameId> AddTradeNameAsync(
        MedicinalProductId market,
        string? language,
        string? name)
    {
        await using var ctx = New();

        var result = await new AddTradeNameHandler(
                new MedicinalProductRepository(ctx))
            .HandleAsync(
                new AddTradeNameCommand(market, language, name), default);

        return result.Id;
    }

    /// <param name="statusDate">
    /// When the market presence began — which is also the business date of its
    /// first market-status entry, so anything a test records afterwards must
    /// come later. A test with a commercial history to tell backdates this.
    /// </param>
    private async Task<MedicinalProductId> CreateAsync(
        RegOSDbContext ctx,
        GlobalProductId globalProductId,
        CountryId? countryId = null,
        DateOnly? statusDate = null)
    {
        var handler = new CreateMedicinalProductHandler(
            new MedicinalProductPolicy(ctx),
            new MedicinalProductRepository(ctx),
            TestTenant.Context);

        var result = await handler.HandleAsync(
            new CreateMedicinalProductCommand(
                globalProductId,
                countryId ?? UnitedStates,
                statusDate ?? Today),
            default);

        _medicinalProductIds.Add(result.Id.Value);

        return result.Id;
    }

    /// <summary>
    /// A real licence over this market, so the "retiring leaves it untouched"
    /// test proves something rather than asserting over an empty set.
    /// </summary>
    private async Task<RegistrationId> RegistrationAsync(
        RegOSDbContext ctx,
        MedicinalProductId market)
    {
        var result = await new CreateRegistrationHandler(
                new RegistrationCreationPolicy(ctx),
                new RegistrationRepository(ctx),
                TestTenant.Context)
            .HandleAsync(
                new CreateRegistrationCommand(market, Fda, Holder, Today),
                default);

        _registrationIds.Add(result.Id.Value);

        return result.Id;
    }

    private async Task<GlobalProductId> ProductAsync(RegOSDbContext ctx)
    {
        var product = GlobalProduct.Register(
            TestTenant.Id,
            $"MED-{Guid.NewGuid():N}"[..20],
            "Medicinal Product Test Product",
            ProductType.Drug);

        ctx.Products.Add(product);
        await ctx.SaveChangesAsync();
        _productIds.Add(product.Id.Value);

        return product.Id;
    }
}
