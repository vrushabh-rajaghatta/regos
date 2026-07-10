namespace RegOS.Product.Domain.Product;

public sealed class Product
{
    public Product(ProductId id, ProductName name, ProductType type)
    {
        Id = id;
        Name = name;
        Type = type;
        Status = ProductStatus.Registered;

    }
    public ProductId Id { get; }
    public ProductName Name { get; }
    public ProductType Type { get; }
    public ProductStatus Status { get; }
}