using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Abstractions;

namespace RegOS.Platform.Application.Tests.Fakes;

/// <summary>
/// A fixed tenant for handler tests. Hand-rolled rather than mocked, matching
/// the convention in <see cref="FakeUserRepository"/> and <see cref="FakeUserPolicy"/>.
/// </summary>
internal sealed class FakeTenantContext : ITenantContext
{
    public FakeTenantContext(OrganizationId organizationId)
        => TenantId = organizationId.Value;

    public FakeTenantContext(Guid tenantId)
        => TenantId = tenantId;

    public Guid TenantId { get; }
}
