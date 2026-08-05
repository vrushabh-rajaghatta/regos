using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.StatePackSupply;

public sealed class StatePackSupplyHandler
{
    private readonly IPackagedProductRepository _packs;

    public StatePackSupplyHandler(IPackagedProductRepository packs)
    {
        _packs = packs;
    }

    public async Task HandleAsync(
        StatePackSupplyCommand command,
        CancellationToken cancellationToken)
    {
        var pack = await _packs.GetByIdAsync(
                command.PackagedProductId, cancellationToken)
            ?? throw new NotFoundException(PackagedProductErrors.NotFound);

        // Built before either is applied, so an unknown code leaves the pack
        // exactly as it was rather than half-restated.
        var shelfLife = ShelfLifeStorage.Create(
            command.ShelfLifeValue,
            PackVocabulary.ShelfLifePeriod(command.ShelfLifeUnitCode),
            command.ShelfLifeText,
            PackVocabulary.StorageConditions(command.StorageConditionCodes),
            PackVocabulary.TestedAt(command.TestedAtCodes));

        var legalStatus = PackVocabulary.LegalStatus(
            command.LegalStatusOfSupplyCode);

        pack.Classify(legalStatus);
        pack.StateShelfLife(shelfLife);

        await _packs.UpdateAsync(pack, cancellationToken);
    }
}
