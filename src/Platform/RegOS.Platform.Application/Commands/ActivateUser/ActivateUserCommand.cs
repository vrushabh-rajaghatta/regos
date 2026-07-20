using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Commands.ActivateUser;

/// <summary>
/// Grants an invited or inactive user access to RegOS. Activation carries no
/// payload - it is a business decision, not a property update. Tenant scoping
/// is ambient, so the command names only the user it acts on.
/// </summary>
public sealed record ActivateUserCommand(UserId UserId);
