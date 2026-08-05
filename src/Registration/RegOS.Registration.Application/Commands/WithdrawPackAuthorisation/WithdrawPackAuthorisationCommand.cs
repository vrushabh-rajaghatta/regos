using RegOS.Registration.Domain.Aggregates.PackAuthorisations;

namespace RegOS.Registration.Application.Commands.WithdrawPackAuthorisation;

/// <summary>
/// Removes an authorisation recorded in error.
/// </summary>
/// <remarks>
/// <b>Not the same act as withdrawing a pack from the market</b>, which is the
/// pack's own dated marketing status. This removes a statement that was never
/// true — a licence the pack was never authorised under — and there is no
/// regulatory record to retain because nothing happened.
/// </remarks>
public sealed record WithdrawPackAuthorisationCommand(
    PackAuthorisationId PackAuthorisationId);
