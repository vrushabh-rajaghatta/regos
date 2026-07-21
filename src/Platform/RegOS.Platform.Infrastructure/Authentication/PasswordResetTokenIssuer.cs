using Microsoft.Extensions.Options;

using RegOS.Platform.Application.Services;

namespace RegOS.Platform.Infrastructure.Authentication;

public sealed class PasswordResetTokenIssuer : IPasswordResetTokenIssuer
{
    private readonly SecretTokenFactory _tokens;
    private readonly PasswordResetOptions _options;

    public PasswordResetTokenIssuer(
        SecretTokenFactory tokens,
        IOptions<PasswordResetOptions> options)
    {
        _tokens = tokens;
        _options = options.Value;
    }

    public IssuedPasswordResetToken Issue(DateTime now)
    {
        var value = _tokens.CreateValue();

        return new IssuedPasswordResetToken(
            value,
            Hash(value),
            now.AddMinutes(_options.Minutes));
    }

    public string Hash(string tokenValue) => _tokens.Hash(tokenValue);
}
