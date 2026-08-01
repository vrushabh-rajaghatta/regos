using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Application.Commands.ResendInvitation;

public sealed record ResendInvitationCommand(UserId UserId);
