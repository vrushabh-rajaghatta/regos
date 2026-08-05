using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Platform.Application.Tests.Fakes;
using RegOS.ReferenceData.Domain.Substances;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Application.Tests.Tenancy;

/// <summary>
/// Proves the <b>second</b> of ADR-038's three filter shapes —
/// <c>TenantId == null || TenantId == CurrentTenant</c> — end to end, using
/// <see cref="Substance"/> as its subject (ADR-058 §2).
/// </summary>
/// <remarks>
/// <see cref="TenantIsolationTests"/> covers the fail-closed shape; nothing
/// covered this one, and the two fail differently. Fail-closed leaks by showing
/// another tenant's row; shared-plus-extensible can also fail by <em>hiding</em>
/// the platform's — a filter written as a bare <c>x.TenantId == CurrentTenant</c>
/// would empty the shared catalogue for everyone and look like an empty table
/// rather than a bug.
/// <para>
/// Every query here is deliberately bare — no handler, no manual
/// <c>.Where(…)</c> — because the claim under test is that the filter does the
/// work on its own.
/// </para>
/// </remarks>
[Collection(PlatformDatabase.Collection)]
public sealed class SharedPlusExtensibleIsolationTests : IAsyncLifetime
{
    private readonly PlatformDatabase _database;

    public SharedPlusExtensibleIsolationTests(PlatformDatabase database)
    {
        _database = database;
    }


    private readonly TenantId _tenantA = TenantId.From(Guid.NewGuid());
    private readonly TenantId _tenantB = TenantId.From(Guid.NewGuid());

    private Substance _shared = default!;
    private Substance _ownedByA = default!;
    private Substance _ownedByB = default!;

    private DbContextOptions<RegOSDbContext> Options() =>
        _database.Options;

    private RegOSDbContext ContextFor(TenantId tenant) =>
        new(Options(), new FakeTenantContext(tenant));

    private RegOSDbContext ContextWithoutIdentity() =>
        new(Options());

    // A fresh concept per call: it is persisted as an owned entity, and EF
    // tracks one against exactly one owner.
    private static CodedConcept Chemical() =>
        CodedConcept.Internal("CHEMICAL", "Chemical");

    private static CodedConcept Synthetic() =>
        CodedConcept.Internal("SYNTHETIC", "Synthetic");

    public async Task InitializeAsync()
    {
        var run = Guid.NewGuid().ToString("N")[..8];

        _shared = Substance.Seed(
            SubstanceId.New(),
            $"Shared-{run}",
            inn: null,
            Chemical(),
            Synthetic());

        _ownedByA = Substance.CreateForTenant(
            _tenantA, $"OwnedByA-{run}", null, Chemical(), Synthetic());

        _ownedByB = Substance.CreateForTenant(
            _tenantB, $"OwnedByB-{run}", null, Chemical(), Synthetic());

        await using var context = ContextWithoutIdentity();
        context.Substances.AddRange(_shared, _ownedByA, _ownedByB);
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = ContextWithoutIdentity();

        // Raw SQL: cleanup must not depend on the very filters under test.
        await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Substances\" WHERE \"Id\" IN ({0}, {1}, {2})",
            _shared.Id.Value, _ownedByA.Id.Value, _ownedByB.Id.Value);
    }

    [Fact]
    public async Task The_shared_catalogue_is_visible_to_every_tenant()
    {
        await using var asA = ContextFor(_tenantA);
        await using var asB = ContextFor(_tenantB);

        // No .Where at all — the filter is the only thing scoping these.
        (await asA.Substances.Select(x => x.Id).ToListAsync())
            .Should().Contain(_shared.Id);

        (await asB.Substances.Select(x => x.Id).ToListAsync())
            .Should().Contain(_shared.Id);
    }

    [Fact]
    public async Task A_tenants_own_compound_is_visible_to_nobody_else()
    {
        await using var context = ContextFor(_tenantA);

        var visible = await context.Substances.Select(x => x.Id).ToListAsync();

        visible.Should().Contain(_ownedByA.Id);
        visible.Should().NotContain(_ownedByB.Id);
    }

    [Fact]
    public async Task The_other_tenant_sees_the_mirror_image()
    {
        await using var context = ContextFor(_tenantB);

        var visible = await context.Substances.Select(x => x.Id).ToListAsync();

        visible.Should().Contain(_ownedByB.Id);
        visible.Should().NotContain(_ownedByA.Id);
    }

    /// <summary>
    /// Fail-closed even here. A caller with no tenant sees nothing — not even
    /// the shared rows — because "no identity" must mean <em>no rows</em>
    /// rather than "the null-tenant rows" (ADR-031).
    /// </summary>
    [Fact]
    public async Task No_identity_sees_nothing_at_all()
    {
        await using var context = ContextWithoutIdentity();

        var visible = await context.Substances.Select(x => x.Id).ToListAsync();

        visible.Should().BeEmpty();
    }
}
