using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.RestatePack;

public sealed class RestatePackHandler
{
    private readonly IPackagedProductRepository _packs;

    public RestatePackHandler(IPackagedProductRepository packs)
    {
        _packs = packs;
    }

    public async Task HandleAsync(
        RestatePackCommand command,
        CancellationToken cancellationToken)
    {
        var pack = await _packs.GetByIdAsync(
                command.PackagedProductId, cancellationToken)
            ?? throw new NotFoundException(PackagedProductErrors.NotFound);

        pack.Describe(
            command.Description,
            command.PackSizeQuantity,
            PackVocabulary.UnitOfPresentation(command.PackSizeUnitCode),
            command.PackCode);

        await _packs.UpdateAsync(pack, cancellationToken);
    }
}
