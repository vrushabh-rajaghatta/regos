using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Application.Commands.RenameTenant;

public sealed record RenameTenantCommand(
    TenantId TenantId,
    string? Name);
