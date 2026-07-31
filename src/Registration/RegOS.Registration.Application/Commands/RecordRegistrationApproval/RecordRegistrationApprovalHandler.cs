using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Registration.Application.Commands.RecordRegistrationApproval;

public sealed class RecordRegistrationApprovalHandler
{
    private readonly IRegistrationRepository _repository;

    public RecordRegistrationApprovalHandler(IRegistrationRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        RecordRegistrationApprovalCommand command,
        CancellationToken cancellationToken)
    {
        var registration = await _repository.GetByIdAsync(
            command.RegistrationId,
            cancellationToken);

        if (registration is null)
            throw new NotFoundException(
                RegistrationRuleErrors.RegistrationDoesNotExist);

        // Every rule about what it means to become approved lives in the
        // aggregate; the handler only resolves the record it applies to.
        registration.RecordApproval(
            command.RegistrationNumber,
            command.ApprovedOn,
            command.ExpiresOn,
            command.Note);

        await _repository.UpdateAsync(registration, cancellationToken);
    }
}
