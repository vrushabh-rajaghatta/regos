namespace RegOS.Platform.Application.Commands.InviteUser;

/// <summary>
/// Invites a person into the caller's own organization. The organization is
/// not a parameter: it is the tenant, and comes from <c>ITenantContext</c>.
/// </summary>
public sealed record InviteUserCommand(
    string FirstName,
    string LastName,
    string Email);
