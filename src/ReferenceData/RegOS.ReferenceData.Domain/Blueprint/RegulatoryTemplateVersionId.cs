namespace RegOS.ReferenceData.Domain.Blueprint;

public readonly record struct RegulatoryTemplateVersionId(Guid Value)
{
    public static RegulatoryTemplateVersionId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(RegulatoryTemplateVersionId id)
        => id.Value;
}
