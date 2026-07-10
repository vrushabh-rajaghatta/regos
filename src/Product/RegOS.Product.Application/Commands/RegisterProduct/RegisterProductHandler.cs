namespace RegOS.Product.Application.Commands.RegisterProduct;

using RegOS.Product.Application.Contracts;
using RegOS.Product.Application.Persistence;
using RegOS.Product.Domain.Product;

public sealed class RegisterProductHandler
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterProductHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _repository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductId> HandleAsync(RegisterProductCommand command, CancellationToken cancellationToken = default)
    {
        var productName = new ProductName(command.Name);
        var productId = new ProductId(Guid.NewGuid());
        var product = new Product(productId, productName, command.Type);

        await _repository.AddAsync(product, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return productId;
    }
}