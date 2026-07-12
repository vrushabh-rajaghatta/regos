namespace RegOS.Product.Domain.Product;

public sealed class Product
{
    private Product(ProductId id, ProductName name, ProductType type)
    {
        Id = id;
        Name = name;
        Type = type;
        Status = ProductStatus.Registered;
    }

    public ProductId Id { get; }
    public ProductName Name { get; private set; }
    public ProductType Type { get; }
    public ProductStatus Status { get; private set; }

    public static Product Create(string name, ProductType type)
    {
        return new Product(
            new ProductId(Guid.NewGuid()),
            new ProductName(name),
            type);
    }

    public void Rename(string name)
    {
        Name = new ProductName(name);
    }

    public void Archive()
    {
        Status = ProductStatus.Archived;
    }
}
