using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Application.Commands.InviteUser;

public sealed record InviteUserResult(
    UserId Id,
    UserStatus Status);
