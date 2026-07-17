namespace RegOS.ProductDocument.Domain.IDs;

public readonly record struct ProductDocumentId(Guid Value)
{
    public static ProductDocumentId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ProductDocumentId id)
        => id.Value;
}
