using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;
using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.Product.Application.Commands.AddTradeName;

public sealed class AddTradeNameHandler
{
    private readonly IMedicinalProductRepository _repository;

    public AddTradeNameHandler(IMedicinalProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<AddTradeNameResult> HandleAsync(
        AddTradeNameCommand command,
        CancellationToken cancellationToken)
    {
        // Parsed before the aggregate is loaded so a malformed code is a 400
        // rather than a round trip that fails on the way out.
        var language = LanguageCode.Parse(command.Language);

        var medicinalProduct = await _repository.GetByIdAsync(
            command.MedicinalProductId, cancellationToken);

        if (medicinalProduct is null)
            throw new NotFoundException(
                MedicinalProductPolicyErrors.MedicinalProductDoesNotExist);

        // The aggregate owns the invariant; the handler never reimplements it.
        var tradeName = medicinalProduct.AddTradeName(language, command.Name);

        await _repository.UpdateAsync(medicinalProduct, cancellationToken);

        return new AddTradeNameResult(tradeName.Id);
    }
}
