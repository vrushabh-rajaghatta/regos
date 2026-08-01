using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.DeactivateMedicinalProduct;

/// <summary>
/// Load, invoke the aggregate behaviour, persist. No policy: nothing outside
/// this aggregate is consulted, deliberately — see
/// <see cref="MedicinalProduct.Deactivate"/>.
/// </summary>
public sealed class DeactivateMedicinalProductHandler
{
    private readonly IMedicinalProductRepository _repository;

    public DeactivateMedicinalProductHandler(
        IMedicinalProductRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        DeactivateMedicinalProductCommand command,
        CancellationToken cancellationToken)
    {
        var medicinalProduct = await _repository.GetByIdAsync(
            command.MedicinalProductId, cancellationToken);

        if (medicinalProduct is null)
            throw new NotFoundException(
                MedicinalProductPolicyErrors.MedicinalProductDoesNotExist);

        medicinalProduct.Deactivate(command.On);

        await _repository.UpdateAsync(medicinalProduct, cancellationToken);
    }
}
