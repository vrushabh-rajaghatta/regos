using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.ValueObjects;

namespace RegOS.Platform.Application.Tests.Fakes;

/// <summary>
/// Records what would have been sent. The plaintext token exists nowhere else
/// after issuing, so this is how a test gets hold of one.
/// </summary>
public sealed class FakeInvitationNotifier : IInvitationNotifier
{
    public Email? Email { get; private set; }

    public string? Token { get; private set; }

    public DateTime? ExpiresAt { get; private set; }

    public int SendCount { get; private set; }

    public Task SendAsync(
        Email email,
        string firstName,
        string token,
        DateTime expiresAt,
        CancellationToken cancellationToken)
    {
        Email = email;
        Token = token;
        ExpiresAt = expiresAt;
        SendCount++;

        return Task.CompletedTask;
    }
}
