using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.UpdateProduct;

/// <summary>
/// The write-side pattern, with no special cases: load the aggregate through
/// the repository, invoke its behaviour, persist. No DbContext, no projection,
/// no unit of work.
/// </summary>
public sealed class UpdateProductHandler
{
    private readonly IProductRepository _repository;
    private readonly ITenantContext _tenantContext;

    public UpdateProductHandler(
        IProductRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task HandleAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(
            command.GlobalProductId, cancellationToken);

        // A product in another tenant is reported as missing, never forbidden,
        // so the API does not reveal that it exists.
        if (product is null || product.TenantId != _tenantContext.TenantId)
            throw new NotFoundException(ProductCommandErrors.ProductNotFound);

        // The aggregate owns the invariants (name required, length) and the
        // intent of each change; the handler never reimplements them.
        product.Rename(command.Name);
        product.ChangeType(command.Type);

        await _repository.UpdateAsync(product, cancellationToken);
    }
}
