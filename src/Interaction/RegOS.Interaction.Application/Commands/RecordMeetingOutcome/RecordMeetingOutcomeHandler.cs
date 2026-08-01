using RegOS.Interaction.Domain.Meetings;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Application.Commands.RecordMeetingOutcome;

public sealed class RecordMeetingOutcomeHandler
{
    private readonly IHaMeetingRepository _repository;

    public RecordMeetingOutcomeHandler(IHaMeetingRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        RecordMeetingOutcomeCommand command,
        CancellationToken cancellationToken)
    {
        var meeting = await _repository.GetByIdAsync(command.MeetingId, cancellationToken)
            ?? throw new NotFoundException("The meeting was not found.");

        meeting.RecordOutcome(command.Minutes, command.Outcome);

        await _repository.UpdateAsync(meeting, cancellationToken);
    }
}
