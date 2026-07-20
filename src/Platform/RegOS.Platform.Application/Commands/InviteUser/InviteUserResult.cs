using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Commands.InviteUser;

public sealed record InviteUserResult(
    UserId Id,
    UserStatus Status);
