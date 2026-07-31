using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Application.Commands.CreateMedicinalProduct;
using RegOS.Product.Application.Queries.ListMedicinalProducts;
using RegOS.Product.Application.Services;
using RegOS.Product.Application.Tests.Fixtures;
using RegOS.Product.Domain.Product;
using RegOS.Product.Infrastructure.Persistence;
using RegOS.Product.Infrastructure.Services;
using RegOS.ReferenceData.Domain.Geography.Country;
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

    // --- Reading -------------------------------------------------------------

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

    private async Task<MedicinalProductId> CreateAsync(
        RegOSDbContext ctx,
        GlobalProductId globalProductId,
        CountryId? countryId = null)
    {
        var handler = new CreateMedicinalProductHandler(
            new MedicinalProductPolicy(ctx),
            new MedicinalProductRepository(ctx),
            TestTenant.Context);

        var result = await handler.HandleAsync(
            new CreateMedicinalProductCommand(
                globalProductId, countryId ?? UnitedStates, Today),
            default);

        _medicinalProductIds.Add(result.Id.Value);

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
