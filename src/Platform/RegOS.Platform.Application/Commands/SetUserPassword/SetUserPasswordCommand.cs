using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Commands.SetUserPassword;

public sealed record SetUserPasswordCommand(UserId UserId, string? Password);
