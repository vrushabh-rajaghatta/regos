namespace RegOS.Platform.Application.Commands.ChangePassword;

/// <summary>
/// Note what is absent: a user id.
/// </summary>
/// <remarks>
/// A caller may only change their own password, and the way to guarantee that
/// is to leave them no way to say whose password they mean. The identity comes
/// from the access token via <c>ICurrentUser</c> — proven, not asserted
/// (ADR-024). Adding a <c>UserId</c> here would create an endpoint one missing
/// check away from letting anyone take over any account.
/// </remarks>
public sealed record ChangePasswordCommand(
    string? CurrentPassword,
    string? NewPassword);
