using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Application.Commands.SetUserPassword;

public sealed record SetUserPasswordCommand(UserId UserId, string? Password);
