using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Geography.Country;

public sealed class Country
{
    private Country()
    {
    }

    public CountryId Id { get; private set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g. US, IN, JP).
    /// </summary>
    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public string? RegionCode { get; private set; }

    public static Country Create(
        CountryId id,
        string code,
        string name,
        string? regionCode = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(CountryErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(CountryErrors.NameRequired);

        return new Country
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            RegionCode = regionCode
        };
    }

    public static Country Create(
        string code,
        string name,
        string? regionCode = null)
        => Create(CountryId.New(), code, name, regionCode);
}
