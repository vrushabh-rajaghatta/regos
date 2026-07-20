using RegOS.Product.Application.Persistence;
using RegOS.Product.Domain.Product;

using ProductAggregate = RegOS.Product.Domain.Product.Product;

namespace RegOS.Product.Application.Commands.RegisterProduct;

public sealed class RegisterProductHandler
{
    private readonly IProductRepository _repository;

    public RegisterProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductId> HandleAsync(
        RegisterProductCommand command,
        CancellationToken cancellationToken)
    {
        // The aggregate owns the invariants (name required, length); the
        // handler never reimplements them.
        var product = ProductAggregate.Register(command.Name, command.Type);

        await _repository.AddAsync(product, cancellationToken);

        return product.Id;
    }
}
