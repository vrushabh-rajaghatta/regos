using Microsoft.Extensions.Options;

using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.Platform.Infrastructure.Authentication;

namespace RegOS.Api.Development;

/// <summary>
/// Writes the acceptance link to the log so a developer can follow it.
/// </summary>
/// <remarks>
/// Registered only when the environment is Development, and guarded at the call
/// site in <c>Program.cs</c> rather than inside this class — the same treatment
/// as <see cref="DevelopmentCredentialSeeder"/>, and for the same reason: this
/// logs a live credential in plaintext, and the guarantee that it never happens
/// elsewhere should be readable where it is wired up.
/// </remarks>
public sealed class DevelopmentInvitationNotifier : IInvitationNotifier
{
    private readonly ILogger<DevelopmentInvitationNotifier> _logger;
    private readonly InvitationOptions _options;

    public DevelopmentInvitationNotifier(
        ILogger<DevelopmentInvitationNotifier> logger,
        IOptions<InvitationOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task SendAsync(
        Email email,
        string firstName,
        string token,
        DateTime expiresAt,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "DEVELOPMENT ONLY - invitation for {Email} was not emailed. "
                + "Acceptance link, valid until {ExpiresAt:u}: {Url}?token={Token}",
            email.Value,
            expiresAt,
            _options.AcceptUrl,
            token);

        return Task.CompletedTask;
    }
}
