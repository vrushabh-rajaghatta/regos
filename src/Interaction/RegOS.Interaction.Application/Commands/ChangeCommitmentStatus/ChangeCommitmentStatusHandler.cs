using RegOS.Interaction.Domain.Commitments;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Application.Commands.ChangeCommitmentStatus;

public sealed class ChangeCommitmentStatusHandler
{
    private readonly ICommitmentRepository _repository;

    public ChangeCommitmentStatusHandler(ICommitmentRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        ChangeCommitmentStatusCommand command,
        CancellationToken cancellationToken)
    {
        var commitment =
            await _repository.GetByIdAsync(command.CommitmentId, cancellationToken)
            ?? throw new NotFoundException("The commitment was not found.");

        commitment.ChangeStatus(command.Target, command.OccurredOn, command.Note);

        await _repository.UpdateAsync(commitment, cancellationToken);
    }
}
