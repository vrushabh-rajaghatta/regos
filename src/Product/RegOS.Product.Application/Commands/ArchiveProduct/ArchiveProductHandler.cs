using RegOS.Product.Application.Commands.UpdateProduct;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.ArchiveProduct;

/// <summary>
/// Load, invoke the aggregate behaviour, persist.
/// </summary>
/// <remarks>
/// There is deliberately no check for existing applications, submissions or
/// documents. Product owns product lifecycle; each downstream context owns
/// whether an archived product is eligible for its own work. Asking three other
/// bounded contexts for permission here would introduce runtime coupling we
/// have not demonstrated a need for, and would centralize rules that belong to
/// the workflows that enforce them.
/// </remarks>
public sealed class ArchiveProductHandler
{
    private readonly IProductRepository _repository;
    private readonly ITenantContext _tenantContext;

    public ArchiveProductHandler(
        IProductRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task HandleAsync(
        ArchiveProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(
            command.GlobalProductId, cancellationToken);

        if (product is null || product.TenantId != _tenantContext.TenantId)
            throw new NotFoundException(ProductCommandErrors.ProductNotFound);

        // The aggregate owns the transition, including refusing a repeat.
        product.Archive();

        await _repository.UpdateAsync(product, cancellationToken);
    }
}
