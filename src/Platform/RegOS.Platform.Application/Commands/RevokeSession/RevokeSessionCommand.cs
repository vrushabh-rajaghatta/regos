namespace RegOS.Platform.Application.Commands.RevokeSession;

/// <param name="SessionId">
/// Null means "every session except the one asking". A user id is deliberately
/// absent: like ChangePasswordCommand, there is nowhere to name somebody else
/// (ADR-024), and ownership is checked against the caller rather than assumed.
/// </param>
public sealed record RevokeSessionCommand(
    Guid? SessionId,
    string? RefreshToken);
