using RegOS.Interaction.Domain.Meetings;
using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Interaction.Application.Commands.AttachHaMeetingToStep;

/// <param name="ProcessStepId">
/// Null clears the link. Clearing is always permitted — an attachment is
/// descriptive, so removing one changes discoverability and nothing else
/// (ADR-065 I9).
/// </param>
public sealed record AttachHaMeetingToStepCommand(
    HaMeetingId HaMeetingId,
    ProcessStepId? ProcessStepId);
