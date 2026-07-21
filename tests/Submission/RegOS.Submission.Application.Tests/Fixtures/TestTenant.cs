using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Submission.Application.Tests.Fixtures;

/// <summary>
/// The single tenant every Submission integration test works under. One fixed
/// tenant rather than a fresh one per fixture: the shared TEST-FIXTURE
/// application (see <see cref="TestApplications"/>) must be findable across
/// test classes, and the global query filter (ADR-031) only shows rows to the
/// tenant that owns them — so every context and every created aggregate must
/// agree on the tenant.
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
