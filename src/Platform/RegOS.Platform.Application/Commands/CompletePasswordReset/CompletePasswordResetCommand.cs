namespace RegOS.Platform.Application.Commands.CompletePasswordReset;

public sealed record CompletePasswordResetCommand(string? Token, string? Password);
