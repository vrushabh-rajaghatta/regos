using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Application.Commands.ConfirmObjectiveMarketRecord;
using RegOS.Process.Application.Tests.Fixtures;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.Process.Infrastructure.Repositories;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Application.Tests;

/// <summary>
/// <b>ADR-065 D8's invariant, proven where it actually runs.</b>
/// </summary>
/// <remarks>
/// <em>Once <c>MedicinalProductId</c> is populated it must reference a record
/// whose global product and country are the ones this objective already holds.</em>
/// <para>
/// The rule belongs to the domain and cannot live in the aggregate, because
/// checking it means loading a <c>MedicinalProduct</c> — the cross-aggregate read
/// ADR-016 keeps out. So it lives in the command handler, and it is tested
/// against a real database rather than a mock: **the check is a query, and a
/// query that returns the wrong rows is exactly the failure worth catching.**
/// </para>
/// </remarks>
[Collection(ProcessDatabase.Collection)]
public sealed class ConfirmObjectiveMarketRecordTests
{
    private static readonly CountryId Japan =
        new(Guid.Parse("10000000-0000-0000-0000-000000000006"));

    private static readonly CountryId UnitedStates =
        new(Guid.Parse("10000000-0000-0000-0000-000000000001"));

    private static readonly DateOnly Stated = new(2026, 8, 6);

    private readonly ProcessDatabase _database;

    public ConfirmObjectiveMarketRecordTests(ProcessDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task A_matching_market_record_is_accepted()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var product = await AProductAsync(context);
        var objective = await AnObjectiveAsync(context, product, Japan);
        var market = await AMarketAsync(context, product, Japan);

        await Handler(context).HandleAsync(
            new ConfirmObjectiveMarketRecordCommand(objective.Id, market),
            CancellationToken.None);

        var saved = await Reload(objective.Id);

        saved.MedicinalProductId.Should().Be(market);
    }

    /// <summary>
    /// <b>The failure the invariant exists to prevent.</b> Attaching a US market
    /// record to a Japan objective would leave the objective claiming two
    /// different markets at once.
    /// </summary>
    [Fact]
    public async Task A_market_record_for_another_country_is_refused()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var product = await AProductAsync(context);
        var objective = await AnObjectiveAsync(context, product, Japan);
        var wrongMarket = await AMarketAsync(context, product, UnitedStates);

        var confirm = async () => await Handler(context).HandleAsync(
            new ConfirmObjectiveMarketRecordCommand(objective.Id, wrongMarket),
            CancellationToken.None);

        await confirm.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(ProcessObjectiveErrors.MarketRecordIsForAnotherMarket);

        (await Reload(objective.Id)).MedicinalProductId.Should().BeNull(
            "a refused confirmation leaves the objective as it was");
    }

    [Fact]
    public async Task A_market_record_for_another_product_is_refused()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var product = await AProductAsync(context);
        var otherProduct = await AProductAsync(context);
        var objective = await AnObjectiveAsync(context, product, Japan);
        var wrongMarket = await AMarketAsync(context, otherProduct, Japan);

        var confirm = async () => await Handler(context).HandleAsync(
            new ConfirmObjectiveMarketRecordCommand(objective.Id, wrongMarket),
            CancellationToken.None);

        await confirm.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(ProcessObjectiveErrors.MarketRecordIsForAnotherMarket);
    }

    /// <summary>
    /// Clearing is unconditional — it says <em>"this is no longer the record that
    /// fulfils the objective"</em>, and must stay possible when the record it
    /// pointed at has been retired.
    /// </summary>
    [Fact]
    public async Task The_link_can_always_be_cleared()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var product = await AProductAsync(context);
        var objective = await AnObjectiveAsync(context, product, Japan);
        var market = await AMarketAsync(context, product, Japan);

        await Handler(context).HandleAsync(
            new ConfirmObjectiveMarketRecordCommand(objective.Id, market),
            CancellationToken.None);

        await using var second = _database.NewContext(TestTenant.Context);

        await Handler(second).HandleAsync(
            new ConfirmObjectiveMarketRecordCommand(objective.Id, null),
            CancellationToken.None);

        (await Reload(objective.Id)).MedicinalProductId.Should().BeNull();
    }

    [Fact]
    public async Task A_market_record_that_does_not_exist_is_a_404()
    {
        await using var context = _database.NewContext(TestTenant.Context);

        var product = await AProductAsync(context);
        var objective = await AnObjectiveAsync(context, product, Japan);

        var confirm = async () => await Handler(context).HandleAsync(
            new ConfirmObjectiveMarketRecordCommand(
                objective.Id, MedicinalProductId.New()),
            CancellationToken.None);

        await confirm.Should().ThrowAsync<NotFoundException>();
    }

    // --- fixtures ------------------------------------------------------------

    private static ConfirmObjectiveMarketRecordHandler Handler(
        RegOSDbContext context)
        => new(new ProcessObjectiveRepository(context), context);

    private async Task<ProcessObjective> Reload(ProcessObjectiveId id)
    {
        await using var context = _database.NewContext(TestTenant.Context);

        return await context.ProcessObjectives.AsNoTracking()
            .FirstAsync(x => x.Id == id);
    }

    private static async Task<GlobalProductId> AProductAsync(
        RegOSDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var product = GlobalProduct.Register(
            TestTenant.Id,
            $"OBJ-{suffix}",
            $"Objective fixture {suffix}",
            ProductType.Drug);

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return product.Id;
    }

    private static async Task<MedicinalProductId> AMarketAsync(
        RegOSDbContext context, GlobalProductId product, CountryId country)
    {
        var market = MedicinalProduct.Create(
            TestTenant.Id, product, country, new DateOnly(2026, 1, 1));

        context.MedicinalProducts.Add(market);
        await context.SaveChangesAsync();

        return market.Id;
    }

    private static async Task<ProcessObjective> AnObjectiveAsync(
        RegOSDbContext context, GlobalProductId product, CountryId country)
    {
        var objective = ProcessObjective.Create(
            TestTenant.Id, product, country, "Obtain approval", Stated);

        context.ProcessObjectives.Add(objective);
        await context.SaveChangesAsync();

        return objective;
    }
}
