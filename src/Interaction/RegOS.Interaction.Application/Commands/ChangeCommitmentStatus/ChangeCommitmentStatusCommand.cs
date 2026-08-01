using RegOS.Interaction.Domain.Commitments;

namespace RegOS.Interaction.Application.Commands.ChangeCommitmentStatus;

public sealed record ChangeCommitmentStatusCommand(
    CommitmentId CommitmentId,
    CommitmentStatus Target,
    DateOnly OccurredOn,
    string? Note);
