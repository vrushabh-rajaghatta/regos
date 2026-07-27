namespace RegOS.ReferenceData.Domain.Blueprint;

public readonly record struct RegulatoryTemplateId(Guid Value)
{
    public static RegulatoryTemplateId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(RegulatoryTemplateId id)
        => id.Value;
}
