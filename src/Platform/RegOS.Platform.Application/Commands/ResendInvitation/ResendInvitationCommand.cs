using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Commands.ResendInvitation;

public sealed record ResendInvitationCommand(UserId UserId);
