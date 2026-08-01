using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.ActivateMedicinalProduct;

public sealed class ActivateMedicinalProductHandler
{
    private readonly IMedicinalProductRepository _repository;

    public ActivateMedicinalProductHandler(
        IMedicinalProductRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        ActivateMedicinalProductCommand command,
        CancellationToken cancellationToken)
    {
        var medicinalProduct = await _repository.GetByIdAsync(
            command.MedicinalProductId, cancellationToken);

        if (medicinalProduct is null)
            throw new NotFoundException(
                MedicinalProductPolicyErrors.MedicinalProductDoesNotExist);

        medicinalProduct.Activate(command.On);

        await _repository.UpdateAsync(medicinalProduct, cancellationToken);
    }
}
