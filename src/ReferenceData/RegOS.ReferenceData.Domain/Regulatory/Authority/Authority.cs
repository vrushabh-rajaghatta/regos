using RegOS.ReferenceData.Domain.Geography.Country;

namespace RegOS.ReferenceData.Domain.Regulatory.Authority;

public sealed class Authority
{
    private Authority()
    {
    }

    public AuthorityId Id { get; private set; }

    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public CountryId CountryId { get; private set; }

    public static Authority Create(
        AuthorityId id,
        string code,
        string name,
        CountryId countryId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(
                AuthorityErrors.CodeRequired,
                nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                AuthorityErrors.NameRequired,
                nameof(name));

        if (countryId == default)
            throw new ArgumentException(
                AuthorityErrors.CountryRequired,
                nameof(countryId));

        return new Authority
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            CountryId = countryId
        };
    }

    public static Authority Create(
        string code,
        string name,
        CountryId countryId)
        => Create(AuthorityId.New(), code, name, countryId);
}
