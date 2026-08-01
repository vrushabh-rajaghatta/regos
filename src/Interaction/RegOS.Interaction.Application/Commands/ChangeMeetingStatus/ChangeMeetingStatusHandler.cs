using RegOS.Interaction.Domain.Meetings;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Application.Commands.ChangeMeetingStatus;

public sealed class ChangeMeetingStatusHandler
{
    private readonly IHaMeetingRepository _repository;

    public ChangeMeetingStatusHandler(IHaMeetingRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        ChangeMeetingStatusCommand command,
        CancellationToken cancellationToken)
    {
        var meeting = await _repository.GetByIdAsync(command.MeetingId, cancellationToken)
            ?? throw new NotFoundException("The meeting was not found.");

        meeting.ChangeStatus(command.Target, command.OccurredOn, command.Note);

        await _repository.UpdateAsync(meeting, cancellationToken);
    }
}
