using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.AddPack;

public sealed class AddPackHandler
{
    private readonly IMedicinalProductRepository _markets;
    private readonly IPackagedProductRepository _packs;
    private readonly ITenantContext _tenantContext;

    public AddPackHandler(
        IMedicinalProductRepository markets,
        IPackagedProductRepository packs,
        ITenantContext tenantContext)
    {
        _markets = markets;
        _packs = packs;
        _tenantContext = tenantContext;
    }

    public async Task<AddPackResult> HandleAsync(
        AddPackCommand command,
        CancellationToken cancellationToken)
    {
        // The market must exist, and the filtered read is what proves it is
        // this tenant's. A pack created against another tenant's market would
        // otherwise be representable.
        _ = await _markets.GetByIdAsync(
                command.MedicinalProductId, cancellationToken)
            ?? throw new NotFoundException(
                MedicinalProductPolicyErrors.MedicinalProductDoesNotExist);

        var pack = PackagedProduct.Create(
            _tenantContext.TenantId,
            command.MedicinalProductId,
            command.Description,
            command.PackSizeQuantity,
            PackVocabulary.UnitOfPresentation(command.PackSizeUnitCode),
            command.PackCode,
            command.StatusDate);

        await _packs.AddAsync(pack, cancellationToken);

        return new AddPackResult(pack.Id);
    }
}
