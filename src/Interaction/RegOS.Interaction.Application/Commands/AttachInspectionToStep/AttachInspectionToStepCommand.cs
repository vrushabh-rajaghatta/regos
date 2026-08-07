using RegOS.Interaction.Domain.Inspections;
using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Interaction.Application.Commands.AttachInspectionToStep;

/// <param name="ProcessStepId">
/// Null clears the link. Clearing is always permitted — an attachment is
/// descriptive, so removing one changes discoverability and nothing else
/// (ADR-065 I9).
/// </param>
public sealed record AttachInspectionToStepCommand(
    InspectionId InspectionId,
    ProcessStepId? ProcessStepId);
