using System.Collections.Concurrent;

using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.ValueObjects;

namespace RegOS.Api.Tests;

/// <summary>
/// Keeps the acceptance tokens that were "sent".
/// </summary>
/// <remarks>
/// The only way a test can obtain one. The plaintext exists for the length of
/// one call and is never persisted — only its SHA-256 is — which is the
/// property under test as much as it is an inconvenience.
/// </remarks>
public sealed class CapturingInvitationNotifier : IInvitationNotifier
{
    private readonly ConcurrentDictionary<string, string> _tokens = new();

    public int SendCount { get; private set; }

    public string TokenFor(string email) =>
        _tokens.TryGetValue(email, out var token)
            ? token
            : throw new InvalidOperationException(
                $"No invitation was sent to {email}.");

    public bool Sent(string email) => _tokens.ContainsKey(email);

    public Task SendAsync(
        Email email,
        string firstName,
        string token,
        DateTime expiresAt,
        CancellationToken cancellationToken)
    {
        _tokens[email.Value] = token;
        SendCount++;

        return Task.CompletedTask;
    }
}
