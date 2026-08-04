using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.ChangePackMarketingStatus;

public sealed class ChangePackMarketingStatusHandler
{
    private readonly IPackagedProductRepository _packs;

    public ChangePackMarketingStatusHandler(IPackagedProductRepository packs)
    {
        _packs = packs;
    }

    public async Task HandleAsync(
        ChangePackMarketingStatusCommand command,
        CancellationToken cancellationToken)
    {
        // GetByIdAsync includes the history on purpose — the rule that business
        // time moves forward compares against it.
        var pack = await _packs.GetByIdAsync(
                command.PackagedProductId, cancellationToken)
            ?? throw new NotFoundException(PackagedProductErrors.NotFound);

        pack.ChangeMarketingStatus(
            command.Status, command.OccurredOn, command.Note);

        await _packs.UpdateAsync(pack, cancellationToken);
    }
}
