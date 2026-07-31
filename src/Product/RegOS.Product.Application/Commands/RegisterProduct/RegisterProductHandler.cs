using RegOS.Product.Application.Persistence;
using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Abstractions;


namespace RegOS.Product.Application.Commands.RegisterProduct;

public sealed class RegisterProductHandler
{
    private readonly IProductPolicy _productPolicy;
    private readonly IProductRepository _repository;
    private readonly ITenantContext _tenantContext;

    public RegisterProductHandler(
        IProductPolicy productPolicy,
        IProductRepository repository,
        ITenantContext tenantContext)
    {
        _productPolicy = productPolicy;
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<GlobalProductId> HandleAsync(
        RegisterProductCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Parse before checking uniqueness so a malformed code is a 400 rather
        // than being compared as-is against stored, normalized codes.
        var code = ProductCode.Create(command.Code);

        await _productPolicy.EnsureCodeIsUniqueAsync(
            tenantId, code, cancellationToken);

        // The aggregate owns the invariants; the handler never reimplements them.
        var product = GlobalProduct.Register(
            tenantId, code.Value, command.Name, command.Type);

        await _repository.AddAsync(product, cancellationToken);

        return product.Id;
    }
}
