namespace RegOS.ReferenceData.Domain.Regulatory.Authority;

public readonly record struct AuthorityDivisionId(Guid Value)
{
    public static AuthorityDivisionId New()
        => new(Guid.NewGuid());

    public override string ToString()
        => Value.ToString();

    public static implicit operator Guid(AuthorityDivisionId id)
        => id.Value;
}
