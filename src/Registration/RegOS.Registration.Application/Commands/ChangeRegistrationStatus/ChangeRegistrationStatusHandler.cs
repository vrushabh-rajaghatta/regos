using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Registration.Application.Commands.ChangeRegistrationStatus;

public sealed class ChangeRegistrationStatusHandler
{
    private readonly IRegistrationRepository _repository;

    public ChangeRegistrationStatusHandler(IRegistrationRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        ChangeRegistrationStatusCommand command,
        CancellationToken cancellationToken)
    {
        var registration = await _repository.GetByIdAsync(
            command.RegistrationId,
            cancellationToken);

        if (registration is null)
            throw new NotFoundException(
                RegistrationRuleErrors.RegistrationDoesNotExist);

        // Which transitions are legal is domain policy, not orchestration: the
        // handler resolves the record and the aggregate decides.
        registration.ChangeStatus(
            command.Status,
            command.OccurredOn,
            command.Note);

        await _repository.UpdateAsync(registration, cancellationToken);
    }
}
