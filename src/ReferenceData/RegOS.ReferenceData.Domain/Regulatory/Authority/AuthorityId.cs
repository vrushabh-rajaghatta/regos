namespace RegOS.ReferenceData.Domain.Regulatory.Authority;

public readonly record struct AuthorityId(Guid Value)
{
    public static AuthorityId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(AuthorityId id)
        => id.Value;
}
