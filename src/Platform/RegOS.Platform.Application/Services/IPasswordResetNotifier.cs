using RegOS.Platform.Domain.ValueObjects;

namespace RegOS.Platform.Application.Services;

/// <summary>
/// Tells someone who has forgotten their password how to choose a new one.
/// </summary>
/// <remarks>
/// Separate from <see cref="IInvitationNotifier"/> rather than one shared
/// notifier. They carry different messages to different audiences, and a single
/// interface would need a parameter saying which kind of message this is — the
/// discriminator that turns a seam into a switch statement. Real delivery is
/// still a slice of its own; this seam exists so that building it changes one
/// class rather than the reset flow.
/// </remarks>
public interface IPasswordResetNotifier
{
    Task SendAsync(
        Email email,
        string firstName,
        string token,
        DateTime expiresAt,
        CancellationToken cancellationToken);
}
