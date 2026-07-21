namespace RegOS.SharedKernel.Exceptions;

/// <summary>
/// The caller has not established who they are. Mapped to HTTP 401.
/// </summary>
/// <remarks>
/// <para>
/// Distinct from <see cref="NotFoundException"/> on purpose. Answering "no such
/// user" for one address and "wrong password" for another turns sign-in into an
/// account enumeration oracle, so every authentication failure — unknown email,
/// wrong password, inactive account, no credential set — raises this one type
/// with one message. The specific reason belongs in logs, never in the
/// response.
/// </para>
/// <para>
/// Not to be reused for authorization. "I know who you are and you may not do
/// this" is a different statement (403) from "I do not know who you are" (401),
/// and a client needs to tell them apart to know whether signing in again would
/// help. See ADR-022.
/// </para>
/// </remarks>
public class AuthenticationFailedException : DomainException
{
    public AuthenticationFailedException(string message)
        : base(message)
    {
    }
}
