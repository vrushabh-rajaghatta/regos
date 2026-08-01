using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Primitives;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Application.Commands.CreateTenant;

public sealed record CreateTenantResult(
    TenantId TenantId,
    UserId AdminUserId);
