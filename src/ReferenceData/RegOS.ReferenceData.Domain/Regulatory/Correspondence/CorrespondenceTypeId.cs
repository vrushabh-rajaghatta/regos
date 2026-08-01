namespace RegOS.ReferenceData.Domain.Regulatory.Correspondence;

public readonly record struct CorrespondenceTypeId(Guid Value)
{
    public static CorrespondenceTypeId New()
        => new(Guid.NewGuid());

    public override string ToString()
        => Value.ToString();

    public static implicit operator Guid(CorrespondenceTypeId id)
        => id.Value;
}
