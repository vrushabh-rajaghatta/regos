using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.CeaseManufacturingOperation;

public sealed class CeaseManufacturingOperationHandler
{
    private readonly IManufacturingOperationRepository _operations;

    public CeaseManufacturingOperationHandler(
        IManufacturingOperationRepository operations)
    {
        _operations = operations;
    }

    public async Task HandleAsync(
        CeaseManufacturingOperationCommand command,
        CancellationToken cancellationToken)
    {
        var operation = await _operations.GetByIdAsync(
                command.ManufacturingOperationId, cancellationToken)
            ?? throw new NotFoundException(ManufacturingOperationErrors.NotFound);

        operation.Cease(command.CeasedOn);

        await _operations.UpdateAsync(operation, cancellationToken);
    }
}
