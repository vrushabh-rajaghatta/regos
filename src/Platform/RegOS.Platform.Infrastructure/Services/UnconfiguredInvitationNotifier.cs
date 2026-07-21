using Microsoft.Extensions.Logging;

using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.ValueObjects;

namespace RegOS.Platform.Infrastructure.Services;

/// <summary>
/// The default notifier: records that an invitation could not be delivered.
/// </summary>
/// <remarks>
/// Deliberately does <b>not</b> log the token. An acceptance token is a
/// credential, and writing credentials into production logs is how they end up
/// in a log aggregator, a screenshot and a support ticket.
///
/// Development replaces this with one that does log the link, because there is
/// no mailbox to read there and the token is the only way in. That swap happens
/// at the composition root, where the environment guard is visible.
/// </remarks>
public sealed class UnconfiguredInvitationNotifier : IInvitationNotifier
{
    private readonly ILogger<UnconfiguredInvitationNotifier> _logger;

    public UnconfiguredInvitationNotifier(
        ILogger<UnconfiguredInvitationNotifier> logger)
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
            "Invitation for {Email} was NOT delivered: no delivery mechanism "
                + "is configured. The user cannot accept until one exists or "
                + "the invitation is resent.",
            email.Value);

        return Task.CompletedTask;
    }
}
