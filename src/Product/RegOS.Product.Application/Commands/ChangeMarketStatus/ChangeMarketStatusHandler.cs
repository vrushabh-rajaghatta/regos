using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.ChangeMarketStatus;

public sealed class ChangeMarketStatusHandler
{
    private readonly IMedicinalProductRepository _repository;

    public ChangeMarketStatusHandler(IMedicinalProductRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        ChangeMarketStatusCommand command,
        CancellationToken cancellationToken)
    {
        var medicinalProduct = await _repository.GetByIdAsync(
            command.MedicinalProductId, cancellationToken);

        if (medicinalProduct is null)
            throw new NotFoundException(
                MedicinalProductPolicyErrors.MedicinalProductDoesNotExist);

        medicinalProduct.ChangeMarketStatus(
            command.Status, command.OccurredOn, command.Note);

        await _repository.UpdateAsync(medicinalProduct, cancellationToken);
    }
}
