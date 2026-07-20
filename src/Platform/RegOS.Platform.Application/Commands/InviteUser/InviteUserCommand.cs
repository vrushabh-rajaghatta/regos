using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Platform.Application.Commands.InviteUser;

public sealed record InviteUserCommand(
    OrganizationId OrganizationId,
    string FirstName,
    string LastName,
    string Email);
