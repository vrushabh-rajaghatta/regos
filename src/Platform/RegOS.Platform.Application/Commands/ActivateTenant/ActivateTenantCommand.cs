using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Application.Commands.ActivateTenant;

public sealed record ActivateTenantCommand(TenantId TenantId);
