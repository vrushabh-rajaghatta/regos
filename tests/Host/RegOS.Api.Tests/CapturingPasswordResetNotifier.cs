using System.Collections.Concurrent;

using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.ValueObjects;

namespace RegOS.Api.Tests;

/// <summary>
/// Keeps the reset tokens that were "sent".
/// </summary>
/// <remarks>
/// The only way a test can obtain one, for the same reason as
/// <see cref="CapturingInvitationNotifier"/>: the plaintext exists for the
/// length of one call and only its SHA-256 is persisted.
///
/// It also answers a question no other layer can — whether a link was sent at
/// all. Requesting a reset always returns 204, so from outside, "sent" and
/// "silently ignored" are indistinguishable. That is the security property; it
/// is also what makes this class necessary.
/// </remarks>
public sealed class CapturingPasswordResetNotifier : IPasswordResetNotifier
{
    private readonly ConcurrentDictionary<string, string> _tokens = new();

    public string TokenFor(string email) =>
        _tokens.TryGetValue(email, out var token)
            ? token
            : throw new InvalidOperationException(
                $"No password reset was sent to {email}.");

    public bool Sent(string email) => _tokens.ContainsKey(email);

    /// <summary>Forgets what was sent, so a test can assert "and not again".</summary>
    public void Forget(string email) => _tokens.TryRemove(email, out _);

    public Task SendAsync(
        Email email,
        string firstName,
        string token,
        DateTime expiresAt,
        CancellationToken cancellationToken)
    {
        _tokens[email.Value] = token;

        return Task.CompletedTask;
    }
}
