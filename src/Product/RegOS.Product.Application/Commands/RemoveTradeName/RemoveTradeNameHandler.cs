using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.RemoveTradeName;

public sealed class RemoveTradeNameHandler
{
    private readonly IMedicinalProductRepository _repository;

    public RemoveTradeNameHandler(IMedicinalProductRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        RemoveTradeNameCommand command,
        CancellationToken cancellationToken)
    {
        var medicinalProduct = await _repository.GetByIdAsync(
            command.MedicinalProductId, cancellationToken);

        if (medicinalProduct is null)
            throw new NotFoundException(
                MedicinalProductPolicyErrors.MedicinalProductDoesNotExist);

        medicinalProduct.RemoveTradeName(command.TradeNameId);

        await _repository.UpdateAsync(medicinalProduct, cancellationToken);
    }
}
