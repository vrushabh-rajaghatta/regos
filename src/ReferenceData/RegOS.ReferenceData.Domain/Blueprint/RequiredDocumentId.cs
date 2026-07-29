namespace RegOS.ReferenceData.Domain.Blueprint;

public readonly record struct RequiredDocumentId(Guid Value)
{
    public static RequiredDocumentId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(RequiredDocumentId id)
        => id.Value;
}
