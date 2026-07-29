namespace RegOS.ReferenceData.Domain.Blueprint;

public readonly record struct ValidationRuleId(Guid Value)
{
    public static ValidationRuleId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ValidationRuleId id)
        => id.Value;
}
