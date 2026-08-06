using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Registration.Application.Commands.AttachRegistrationToStep;

/// <param name="ProcessStepId">
/// Null clears the link. Clearing is always permitted — an attachment is
/// descriptive, so removing one changes discoverability and nothing else
/// (ADR-065 I9).
/// </param>
public sealed record AttachRegistrationToStepCommand(
    RegistrationId RegistrationId,
    ProcessStepId? ProcessStepId);
