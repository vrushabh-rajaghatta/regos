using Microsoft.Extensions.Options;

using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.Platform.Infrastructure.Authentication;

namespace RegOS.Api.Development;

/// <summary>
/// Writes the password reset link to the log so a developer can follow it.
/// </summary>
/// <remarks>
/// Registered only when the environment is Development, and guarded at the call
/// site in <c>Program.cs</c> rather than inside this class — the same treatment
/// as <see cref="DevelopmentInvitationNotifier"/>, and for the same reason: it
/// logs a live credential in plaintext, and the guarantee that this never
/// happens elsewhere should be readable where it is wired up.
/// </remarks>
public sealed class DevelopmentPasswordResetNotifier : IPasswordResetNotifier
{
    private readonly ILogger<DevelopmentPasswordResetNotifier> _logger;
    private readonly PasswordResetOptions _options;

    public DevelopmentPasswordResetNotifier(
        ILogger<DevelopmentPasswordResetNotifier> logger,
        IOptions<PasswordResetOptions> options)
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
            "DEVELOPMENT ONLY - password reset for {Email} was not emailed. "
                + "Reset link, valid until {ExpiresAt:u}: {Url}?token={Token}",
            email.Value,
            expiresAt,
            _options.CompleteUrl,
            token);

        return Task.CompletedTask;
    }
}
