using RegOS.Platform.Domain.Aggregates.Tenant;

namespace RegOS.Platform.Application.Queries.GetTenants;

public sealed record TenantListItem(
    Guid Id,
    string Name,
    TenantStatus Status);
