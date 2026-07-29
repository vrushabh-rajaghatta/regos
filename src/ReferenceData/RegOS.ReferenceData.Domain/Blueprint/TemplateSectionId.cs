namespace RegOS.ReferenceData.Domain.Blueprint;

public readonly record struct TemplateSectionId(Guid Value)
{
    public static TemplateSectionId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(TemplateSectionId id)
        => id.Value;
}
