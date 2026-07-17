namespace RegOS.ReferenceData.Domain.Geography.Country;

public readonly record struct CountryId(Guid Value)
{
    public static CountryId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CountryId id) => id.Value;
}
