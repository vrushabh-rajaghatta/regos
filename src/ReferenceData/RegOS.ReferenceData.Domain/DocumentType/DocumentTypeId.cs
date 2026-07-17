namespace RegOS.ReferenceData.Domain.DocumentType;

public readonly record struct DocumentTypeId(Guid Value)
{
    public static DocumentTypeId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(DocumentTypeId id)
        => id.Value;
}
