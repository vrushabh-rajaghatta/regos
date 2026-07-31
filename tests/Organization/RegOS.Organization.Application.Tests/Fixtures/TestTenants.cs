using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Organization.Application.Tests.Fixtures;

/// <summary>
/// Two tenants, because this story's central claim is that one cannot see the
/// other's sites. A single tenant could never prove it.
/// </summary>
internal static class TestTenants
{
    /// <summary>Demo Manufacturer Ltd. — a tenant the seed data guarantees.</summary>
    public static readonly TenantId Acting =
        new(Guid.Parse("30000000-0000-0000-0000-000000000001"));

    /// <summary>A different seeded tenant, used only to look and find nothing.</summary>
    public static readonly TenantId Other =
        new(Guid.Parse("30000000-0000-0000-0000-000000000003"));

    public static ITenantContext ContextFor(TenantId id) => new Fixed(id);

    public static readonly ITenantContext ActingContext = ContextFor(Acting);

    public static readonly ITenantContext OtherContext = ContextFor(Other);

    private sealed class Fixed : ITenantContext
    {
        private readonly TenantId _id;

        public Fixed(TenantId id) => _id = id;

        public TenantId TenantId => _id;

        public TenantId? TenantIdOrNull => _id;
    }
}
