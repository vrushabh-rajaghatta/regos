using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Application.Tests.Fakes;

/// <summary>
/// A fixed tenant for handler tests. Hand-rolled rather than mocked, matching
/// the convention in <see cref="FakeUserRepository"/> and <see cref="FakeUserPolicy"/>.
/// </summary>
internal sealed class FakeTenantContext : ITenantContext
{
    public FakeTenantContext(TenantId tenantId)
        => TenantId = tenantId;

    public FakeTenantContext(Guid tenantId)
        => TenantId = new TenantId(tenantId);

    public TenantId TenantId { get; }

    public TenantId? TenantIdOrNull => TenantId;
}
