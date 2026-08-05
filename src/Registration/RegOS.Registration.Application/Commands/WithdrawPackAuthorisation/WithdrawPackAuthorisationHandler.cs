using RegOS.Registration.Domain.Aggregates.PackAuthorisations;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Registration.Application.Commands.WithdrawPackAuthorisation;

public sealed class WithdrawPackAuthorisationHandler
{
    private readonly IPackAuthorisationRepository _authorisations;

    public WithdrawPackAuthorisationHandler(
        IPackAuthorisationRepository authorisations)
    {
        _authorisations = authorisations;
    }

    public async Task HandleAsync(
        WithdrawPackAuthorisationCommand command,
        CancellationToken cancellationToken)
    {
        var authorisation = await _authorisations.GetByIdAsync(
                command.PackAuthorisationId, cancellationToken)
            ?? throw new NotFoundException(PackAuthorisationErrors.NotFound);

        await _authorisations.RemoveAsync(authorisation, cancellationToken);
    }
}
