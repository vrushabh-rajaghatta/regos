namespace RegOS.ReferenceData.Domain.Organization;

public readonly record struct IdentifierSchemeId(Guid Value)
{
    public static IdentifierSchemeId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(IdentifierSchemeId id) => id.Value;
}
