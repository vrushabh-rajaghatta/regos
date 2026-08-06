using RegOS.Interaction.Domain.Commitments;
using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Interaction.Application.Commands.AttachCommitmentToStep;

/// <param name="ProcessStepId">
/// Null clears the link. Clearing is always permitted — an attachment is
/// descriptive, so removing one changes discoverability and nothing else
/// (ADR-065 I9).
/// </param>
public sealed record AttachCommitmentToStepCommand(
    CommitmentId CommitmentId,
    ProcessStepId? ProcessStepId);
