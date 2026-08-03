using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Platform.Application.Tests.Tenancy;

/// <summary>
/// Architecture tests over the EF model itself: isolation rules that must
/// hold for every entity, including ones added after this file was written.
/// A new tenant-owned aggregate cannot silently join the model unfiltered —
/// it fails here first.
/// </summary>
public sealed class TenantFilterArchitectureTests
{
    // Model building needs a provider but no database; nothing here connects.
    private static RegOSDbContext BareContext() =>
        new(new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql("Host=localhost;Database=model-only")
            .Options);

    /// <summary>
    /// Tier-1 reference data (ADR-031): world facts and the tenant directory.
    /// Deliberately unfiltered AND deliberately without a tenant column —
    /// adding either would be a modelling error, not an improvement.
    /// Organization left this list at ADR-032: the registry is a tenant's
    /// business relationships, so it is tenant-owned like any other data —
    /// the first test above now *requires* its filter instead.
    /// </summary>
    private static readonly string[] DeliberatelyGlobal =
    [
        "Tenant",
        "Country",
        "Authority",
        "ApplicationType"
    ];

    /// <summary>
    /// Person-scoped satellites: no tenant column, reachable only by user id
    /// or unguessable token hash. Filtering them breaks authentication.
    /// </summary>
    private static readonly string[] PersonScoped =
    [
        "UserCredential",
        "RefreshToken",
        "Invitation",
        "PasswordReset",
        "Session"
    ];

    [Fact]
    public void Every_entity_with_a_tenant_column_has_a_query_filter()
    {
        using var context = BareContext();

        var unprotected = context.Model.GetEntityTypes()
            .Where(e => e.FindProperty("TenantId") is not null)
            .Where(e => e.GetDeclaredQueryFilters().Count == 0)
            .Select(e => e.ClrType.Name)
            .ToList();

        unprotected.Should().BeEmpty(
            "an entity that carries a TenantId but no query filter is a "
            + "tenant-isolation hole (ADR-031); add a filter in "
            + "RegOSDbContext.ApplyTenantFilters or remove the column");
    }

    [Fact]
    public void Tier_one_reference_data_has_neither_tenant_column_nor_filter()
    {
        using var context = BareContext();

        foreach (var name in DeliberatelyGlobal)
        {
            var entity = context.Model.GetEntityTypes()
                .SingleOrDefault(e => e.ClrType.Name == name);

            entity.Should().NotBeNull(
                $"{name} is expected in the model; if it was renamed, "
                + "update this list deliberately");

            entity!.FindProperty("TenantId").Should().BeNull(
                $"{name} is global by design (ADR-031's tier model); a tenant "
                + "column here means someone 'fixed' shared reference data "
                + "by copying it per tenant — see the three-tier decision "
                + "before proceeding");

            entity.GetDeclaredQueryFilters().Should().BeEmpty(
                $"{name} is deliberately visible to every tenant");
        }
    }

    [Fact]
    public void Person_scoped_satellites_stay_outside_the_tenant_filter()
    {
        using var context = BareContext();

        foreach (var name in PersonScoped)
        {
            var entity = context.Model.GetEntityTypes()
                .SingleOrDefault(e => e.ClrType.Name == name);

            entity.Should().NotBeNull();

            entity!.FindProperty("TenantId").Should().BeNull(
                $"{name} belongs to a person, not a tenant (ADR-029/031); "
                + "it is reachable only by user id or token hash");
        }
    }
}
