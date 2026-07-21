namespace RegOS.Platform.Application.Commands.Login;

/// <param name="UserAgent">
/// The browser's own description of itself, and the address it connected from.
/// Both come from the transport rather than the request body — a caller must not
/// be able to choose what their own session says about them — and both are
/// optional, because a non-browser client legitimately has neither (ADR-029).
/// </param>
public sealed record LoginCommand(
    string? Email,
    string? Password,
    string? UserAgent = null,
    string? IpAddress = null);
