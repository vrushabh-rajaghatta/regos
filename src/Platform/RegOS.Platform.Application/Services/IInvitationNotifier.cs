using RegOS.Platform.Domain.ValueObjects;

namespace RegOS.Platform.Application.Services;

/// <summary>
/// Tells an invited person how to accept.
/// </summary>
/// <remarks>
/// An abstraction with one logging implementation, and that is deliberate:
/// real delivery — SMTP, SendGrid, SES, Graph — is a slice of its own, and
/// <c>InviteUserHandler</c> must not know which one a deployment chose either
/// way. The seam exists so that building delivery later changes one class
/// rather than the invitation flow (ADR-027).
/// </remarks>
public interface IInvitationNotifier
{
    Task SendAsync(
        Email email,
        string firstName,
        string token,
        DateTime expiresAt,
        CancellationToken cancellationToken);
}
