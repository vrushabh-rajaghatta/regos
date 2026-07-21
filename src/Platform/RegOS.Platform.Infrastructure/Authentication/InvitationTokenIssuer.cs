using Microsoft.Extensions.Options;

using RegOS.Platform.Application.Services;

namespace RegOS.Platform.Infrastructure.Authentication;

public sealed class InvitationTokenIssuer : IInvitationTokenIssuer
{
    private readonly SecretTokenFactory _tokens;
    private readonly InvitationOptions _options;

    public InvitationTokenIssuer(
        SecretTokenFactory tokens,
        IOptions<InvitationOptions> options)
    {
        _tokens = tokens;
        _options = options.Value;
    }

    public IssuedInvitationToken Issue(DateTime now)
    {
        var value = _tokens.CreateValue();

        return new IssuedInvitationToken(
            value,
            Hash(value),
            now.AddDays(_options.Days));
    }

    public string Hash(string tokenValue) => _tokens.Hash(tokenValue);
}
