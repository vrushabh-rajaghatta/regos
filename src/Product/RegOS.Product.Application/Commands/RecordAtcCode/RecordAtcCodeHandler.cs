using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.RecordAtcCode;

public sealed class RecordAtcCodeHandler
{
    private readonly IMedicinalProductRepository _markets;

    public RecordAtcCodeHandler(IMedicinalProductRepository markets)
    {
        _markets = markets;
    }

    public async Task HandleAsync(
        RecordAtcCodeCommand command,
        CancellationToken cancellationToken)
    {
        var market = await _markets.GetByIdAsync(
                command.MedicinalProductId, cancellationToken)
            ?? throw new NotFoundException(
                MedicinalProductPolicyErrors.MedicinalProductDoesNotExist);

        // The shape check lives in AtcCode itself, not here: it is a fact about
        // what an ATC code is, not about this request. RegOS checks the shape
        // only — it holds no WHO ATC index to check membership against.
        market.RecordAtcCode(command.AtcCode);

        await _markets.UpdateAsync(market, cancellationToken);
    }
}
