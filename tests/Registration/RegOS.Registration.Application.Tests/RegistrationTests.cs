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
using RegOS.Registration.Application.Queries.ListProductRegistrations;
using RegOS.Registration.Application.Tests.Fixtures;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.Registration.Infrastructure.Repositories;
using RegOS.Registration.Infrastructure.Services;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;

using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;
using ProductAggregate = RegOS.Product.Domain.Product.Product;
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
                .FirstOrDefaultAsync(x => x.Id == new ProductId(id));

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
        var productId = await ProductAsync(ctx);

        var id = await CreateAsync(ctx, productId);

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
        var productId = await ProductAsync(ctx);

        await CreateAsync(ctx, productId);
        var second = async () => await CreateAsync(ctx, productId);

        await second.Should().NotThrowAsync();

        await using var check = New();
        var count = await check.Registrations
            .AsNoTracking()
            .CountAsync(x => x.ProductId == productId);

        count.Should().Be(2);
    }

    [Fact]
    public async Task AnUnknownProductIsNotFound()
    {
        await using var ctx = New();

        var create = async () => await CreateAsync(ctx, ProductId.New());

        await create.Should().ThrowAsync<NotFoundException>()
            .WithMessage(RegistrationRuleErrors.ProductDoesNotExist);
    }

    [Fact]
    public async Task TheAuthorityMustBelongToTheCountry()
    {
        await using var ctx = New();
        var productId = await ProductAsync(ctx);

        // A country the FDA does not belong to.
        var elsewhere = await ctx.Countries
            .AsNoTracking()
            .Where(x => x.Id != UnitedStates)
            .Select(x => x.Id)
            .FirstAsync();

        var create = async () => await CreateAsync(ctx, productId, countryId: elsewhere);

        await create.Should().ThrowAsync<DomainException>()
            .WithMessage(RegistrationRuleErrors.AuthorityNotInCountry);
    }

    [Fact]
    public async Task TheHolderMustBeActive()
    {
        await using var ctx = New();
        var productId = await ProductAsync(ctx);

        var retired = OrganizationAggregate.Create(
            TestTenant.Id, $"Retired Holder {Guid.NewGuid()}", OrganizationType.Manufacturer);
        retired.Deactivate();

        ctx.Organizations.Add(retired);
        await ctx.SaveChangesAsync();
        _organizationIds.Add(retired.Id.Value);

        var create = async () =>
            await CreateAsync(ctx, productId, holderId: retired.Id);

        await create.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(RegistrationRuleErrors.OrganizationInactive);
    }

    // --- Provenance ----------------------------------------------------------

    [Fact]
    public async Task AnAuthorisationFiledElsewhereNeedsNoApplication()
    {
        await using var ctx = New();
        var productId = await ProductAsync(ctx);

        var id = await CreateAsync(ctx, productId);

        await using var check = New();
        var registration = await check.Registrations
            .AsNoTracking().FirstAsync(x => x.Id == id);

        registration.OriginatingApplicationId.Should().BeNull();
    }

    [Fact]
    public async Task TheOriginatingApplicationIsRecordedWhenNamed()
    {
        await using var ctx = New();
        var productId = await ProductAsync(ctx);
        var applicationId = await ApplicationAsync(ctx, productId);

        var id = await CreateAsync(ctx, productId, applicationId: applicationId);

        await using var check = New();
        var registration = await check.Registrations
            .AsNoTracking().FirstAsync(x => x.Id == id);

        registration.OriginatingApplicationId.Should().Be(applicationId);
    }

    [Fact]
    public async Task AnApplicationForAnotherProductIsRejected()
    {
        await using var ctx = New();
        var productId = await ProductAsync(ctx);
        var otherProductId = await ProductAsync(ctx);
        var foreignApplication = await ApplicationAsync(ctx, otherProductId);

        var create = async () =>
            await CreateAsync(ctx, productId, applicationId: foreignApplication);

        await create.Should().ThrowAsync<DomainException>()
            .WithMessage(RegistrationRuleErrors.ApplicationNotForProduct);
    }

    // --- Recording the grant -------------------------------------------------

    [Fact]
    public async Task RecordingApprovalPersistsTheNumberDatesAndASecondHistoryEntry()
    {
        await using var ctx = New();
        var productId = await ProductAsync(ctx);

        // A migrated authorisation: both entries carry their 2019 business
        // dates, in the order they happened, while both are recorded today.
        var id = await CreateAsync(
            ctx, productId, occurredOn: new DateOnly(2019, 1, 15));

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
        var productId = await ProductAsync(ctx);
        var id = await CreateAsync(
            ctx, productId, occurredOn: new DateOnly(2020, 1, 10));

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
        var productId = await ProductAsync(ctx);
        var id = await CreateAsync(ctx, productId);

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
        var productId = await ProductAsync(ctx);
        var id = await CreateAsync(ctx, productId);

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
        var productId = await ProductAsync(ctx);
        var id = await CreateAsync(ctx, productId);

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
        var productId = await ProductAsync(ctx);
        var id = await CreateAsync(ctx, productId);

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
        var productId = await ProductAsync(ctx);
        await CreateAsync(ctx, productId);

        await using var check = New();
        var rows = await new ListProductRegistrationsHandler(check)
            .HandleAsync(productId, default);

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
            .HandleAsync(ProductId.New(), default);

        rows.Should().BeNull();
    }

    // --- helpers -------------------------------------------------------------

    private async Task<RegistrationId> CreateAsync(
        RegOSDbContext ctx,
        ProductId productId,
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
                productId,
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

    private async Task<ProductId> ProductAsync(RegOSDbContext ctx)
    {
        var product = ProductAggregate.Register(
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
        RegOSDbContext ctx, ProductId productId)
    {
        var application = RegulatoryApplicationAggregate.Create(
            TestTenant.Id, productId, UnitedStates, Fda, Holder,
            "Registration Test Application");

        ctx.RegulatoryApplications.Add(application);
        await ctx.SaveChangesAsync();
        _applicationIds.Add(application.Id.Value);

        return application.Id;
    }
}
