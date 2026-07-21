using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Application.Commands.DeactivateTenant;

public sealed record DeactivateTenantCommand(TenantId TenantId);
