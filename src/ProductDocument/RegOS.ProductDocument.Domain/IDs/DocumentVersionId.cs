namespace RegOS.ProductDocument.Domain.IDs;

public readonly record struct DocumentVersionId(Guid Value)
{
    public static DocumentVersionId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(DocumentVersionId id)
        => id.Value;
}
