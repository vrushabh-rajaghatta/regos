using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Registration.Application.Commands.RecordRegistrationApproval;

/// <param name="ApprovedOn">
/// The date the authority granted it — the business date, never the clock, so a
/// 2019 authorisation entered today records 2019.
/// </param>
public sealed record RecordRegistrationApprovalCommand(
    RegistrationId RegistrationId,
    string RegistrationNumber,
    DateOnly ApprovedOn,
    DateOnly? ExpiresOn = null,
    string? Note = null);
