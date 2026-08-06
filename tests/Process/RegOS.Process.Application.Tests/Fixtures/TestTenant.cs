using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Application.Tests.Fixtures;

/// <summary>
/// The single tenant every Process integration test works under — the same one
/// the Registration, Product and Submission tests use.
/// </summary>
/// <remarks>
/// It matters more here than elsewhere, because playbooks take ADR-031's
/// <em>shared-plus-extensible</em> filter: with no tenant at all the filter
/// shows nothing, so a test that forgot to supply one would see an empty table
/// and could not tell that from a seed that never ran.
/// </remarks>
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
