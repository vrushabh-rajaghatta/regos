using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Application.Tests.Fixtures;

/// <summary>
/// The single tenant every Product integration test works under — the same one
/// the Registration tests use, and for the same reason: the global query filter
/// (ADR-031) only shows rows to the tenant that owns them, so every context and
/// every created aggregate must agree on it.
/// </summary>
internal static class TestTenant
{
    /// <summary>Demo Manufacturer Ltd. — a tenant the seed data guarantees.</summary>
    public static readonly TenantId Id =
        new(Guid.Parse("30000000-0000-0000-0000-000000000001"));

    public static readonly ITenantContext Context = new FixedTenantContext();

    private sealed class FixedTenantContext : ITenantContext
    {
        public TenantId TenantId => Id;

        public TenantId? TenantIdOrNull => Id;
    }
}
