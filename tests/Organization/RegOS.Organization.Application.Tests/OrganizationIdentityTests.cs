using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Application.Commands.AddOrganizationIdentifier;
using RegOS.Organization.Application.Commands.RemoveOrganizationIdentifier;
using RegOS.Organization.Application.Commands.UpdateOrganization;
using RegOS.Organization.Application.Queries.Organizations.GetOrganization;
using RegOS.Organization.Application.Services;
using RegOS.Organization.Application.Tests.Fixtures;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Infrastructure.Persistence;
using RegOS.Organization.Infrastructure.Services;
using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

using OrganizationAggregate =
    RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Organization.Application.Tests;

/// <summary>
/// The identity an organization carries, exercised the way a user reaches it —
/// through commands and the read model, not by calling the aggregate directly.
/// </summary>
/// <remarks>
/// EPIC-016 S003 added <c>Acronym</c>, <c>NameNativeLanguage</c>,
/// <c>StatusDate</c> and the identifier collection to the aggregate, and every
/// test it shipped invoked them on an in-memory instance. They all passed while
/// no command wrote them and no projection returned them. These tests fail if
/// that regresses, because they go through the handlers.
/// </remarks>
public sealed class OrganizationIdentityTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    /// <summary>Seeded by the reference-data initialiser.</summary>
    private static readonly IdentifierSchemeId Duns =
        new(Guid.Parse("80000000-0000-0000-0000-000000000001"));

    private readonly List<Guid> _organizationIds = [];

    private static RegOSDbContext New(ITenantContext? tenant = null) =>
        new(
            new DbContextOptionsBuilder<RegOSDbContext>()
                .UseNpgsql(ConnectionString).Options,
            tenant ?? TestTenants.ActingContext);

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var ctx = New();

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
    public async Task TheOverviewCarriesTheIdentityAttributes()
    {
        await using var ctx = New();
        var id = await OrganizationAsync(ctx);

        await UpdateAsync(ctx, id, "DML", "デモ製薬株式会社");

        var details = await ReadAsync(id);

        details.Acronym.Should().Be("DML");
        details.NameNativeLanguage.Should().Be("デモ製薬株式会社");
        details.StatusDate.Should().NotBe(default);
    }

    /// <summary>
    /// The scheme code is what makes an identifier readable: a reader needs
    /// "DUNS 150483782", not two guids.
    /// </summary>
    [Fact]
    public async Task AnIdentifierIsReadBackWithItsSchemeCode()
    {
        await using var ctx = New();
        var id = await OrganizationAsync(ctx);

        await AddAsync(ctx, id, Duns, "150483782");

        var details = await ReadAsync(id);

        var identifier = details.Identifiers.Should().ContainSingle().Subject;

        identifier.Value.Should().Be("150483782");
        identifier.SchemeCode.Should().Be("DUNS");
        identifier.SchemeName.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ASecondIdentifierForTheSameSchemeIsRefused()
    {
        await using var ctx = New();
        var id = await OrganizationAsync(ctx);

        await AddAsync(ctx, id, Duns, "150483782");

        var again = async () => await AddAsync(ctx, id, Duns, "999999999");

        await again.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(OrganizationErrors.IdentifierSchemeAlreadyRecorded);
    }

    /// <summary>
    /// The invariant only holds if the aggregate was loaded whole. This test is
    /// the reason <c>GetByIdAsync</c> includes the identifiers: with a partial
    /// load the duplicate reaches the unique index and surfaces as a raw
    /// persistence failure instead of a stated business rule.
    /// </summary>
    [Fact]
    public async Task TheDuplicateRuleSurvivesAReload()
    {
        await using var ctx = New();
        var id = await OrganizationAsync(ctx);
        await AddAsync(ctx, id, Duns, "150483782");

        await using var fresh = New();
        var again = async () => await AddAsync(fresh, id, Duns, "999999999");

        await again.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task AnUnknownSchemeIsRefused()
    {
        await using var ctx = New();
        var id = await OrganizationAsync(ctx);

        var add = async () =>
            await AddAsync(ctx, id, IdentifierSchemeId.New(), "150483782");

        await add.Should().ThrowAsync<DomainException>()
            .WithMessage(OrganizationIdentifierRuleErrors.SchemeDoesNotExist);
    }

    [Fact]
    public async Task AWithdrawnIdentifierIsGone()
    {
        await using var ctx = New();
        var id = await OrganizationAsync(ctx);
        var identifierId = await AddAsync(ctx, id, Duns, "150483782");

        await using var removing = New();
        await new RemoveOrganizationIdentifierHandler(
                new OrganizationRepository(removing))
            .HandleAsync(
                new RemoveOrganizationIdentifierCommand(id, identifierId),
                default);

        (await ReadAsync(id)).Identifiers.Should().BeEmpty();
    }

    [Fact]
    public async Task WithdrawingAnIdentifierThatIsNotThereIsNotFound()
    {
        await using var ctx = New();
        var id = await OrganizationAsync(ctx);

        var remove = async () =>
            await new RemoveOrganizationIdentifierHandler(
                    new OrganizationRepository(ctx))
                .HandleAsync(
                    new RemoveOrganizationIdentifierCommand(
                        id, OrganizationIdentifierId.New()),
                    default);

        await remove.Should().ThrowAsync<NotFoundException>()
            .WithMessage(OrganizationErrors.IdentifierNotFound);
    }

    /// <summary>
    /// Identity is tenant-scoped like everything else: the richer projection did
    /// not open a door around the query filter (ADR-031).
    /// </summary>
    [Fact]
    public async Task AnotherTenantCannotReadThisOrganization()
    {
        await using var ctx = New();
        var id = await OrganizationAsync(ctx);
        await AddAsync(ctx, id, Duns, "150483782");

        await using var intruder = New(TestTenants.OtherContext);

        var read = async () => await new GetOrganizationHandler(intruder)
            .HandleAsync(new GetOrganizationQuery(id), default);

        await read.Should().ThrowAsync<NotFoundException>();
    }

    private async Task<OrganizationDetails> ReadAsync(OrganizationId id)
    {
        await using var check = New();

        return await new GetOrganizationHandler(check)
            .HandleAsync(new GetOrganizationQuery(id), default);
    }

    private static Task UpdateAsync(
        RegOSDbContext ctx,
        OrganizationId id,
        string? acronym,
        string? nameNativeLanguage)
        => new UpdateOrganizationHandler(new OrganizationRepository(ctx))
            .HandleAsync(
                new UpdateOrganizationCommand(
                    id,
                    "Identity Under Test",
                    OrganizationType.Manufacturer,
                    acronym,
                    nameNativeLanguage),
                default);

    private static Task<OrganizationIdentifierId> AddAsync(
        RegOSDbContext ctx,
        OrganizationId id,
        IdentifierSchemeId schemeId,
        string value)
        => new AddOrganizationIdentifierHandler(
                new OrganizationIdentifierPolicy(ctx),
                new OrganizationRepository(ctx))
            .HandleAsync(
                new AddOrganizationIdentifierCommand(id, schemeId, value),
                default);

    private async Task<OrganizationId> OrganizationAsync(RegOSDbContext ctx)
    {
        var organization = OrganizationAggregate.Create(
            TestTenants.Acting,
            $"Identity Under Test {Guid.NewGuid():N}",
            OrganizationType.Manufacturer);

        ctx.Organizations.Add(organization);
        await ctx.SaveChangesAsync();
        _organizationIds.Add(organization.Id.Value);

        return organization.Id;
    }
}
