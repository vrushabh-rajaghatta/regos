using Microsoft.Extensions.Logging;

using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.ValueObjects;

namespace RegOS.Platform.Infrastructure.Services;

/// <summary>
/// The default notifier: records that a reset link could not be delivered.
/// </summary>
/// <remarks>
/// Deliberately does <b>not</b> log the token, for the same reason
/// <see cref="UnconfiguredInvitationNotifier"/> does not: a reset token is a
/// credential, and credentials in production logs end up in a log aggregator, a
/// screenshot and a support ticket.
///
/// Development replaces this with one that does log the link, because there is
/// no mailbox to read there. That swap happens at the composition root, where
/// the environment guard is visible.
/// </remarks>
public sealed class UnconfiguredPasswordResetNotifier : IPasswordResetNotifier
{
    private readonly ILogger<UnconfiguredPasswordResetNotifier> _logger;

    public UnconfiguredPasswordResetNotifier(
        ILogger<UnconfiguredPasswordResetNotifier> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(
        Email email,
        string firstName,
        string token,
        DateTime expiresAt,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            "Password reset for {Email} was NOT delivered: no delivery "
                + "mechanism is configured. The user cannot recover their "
                + "account until one exists.",
            email.Value);

        return Task.CompletedTask;
    }
}
