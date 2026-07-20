using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Application.Persistence;
using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Abstractions;

using ProductAggregate = RegOS.Product.Domain.Product.Product;

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

    public async Task<ProductId> HandleAsync(
        RegisterProductCommand command,
        CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId(_tenantContext.TenantId);

        // Parse before checking uniqueness so a malformed code is a 400 rather
        // than being compared as-is against stored, normalized codes.
        var code = ProductCode.Create(command.Code);

        await _productPolicy.EnsureCodeIsUniqueAsync(
            organizationId, code, cancellationToken);

        // The aggregate owns the invariants; the handler never reimplements them.
        var product = ProductAggregate.Register(
            organizationId, code.Value, command.Name, command.Type);

        await _repository.AddAsync(product, cancellationToken);

        return product.Id;
    }
}
