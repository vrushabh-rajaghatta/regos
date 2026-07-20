using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.SharedKernel.Exceptions;

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
            throw new DomainException(AuthorityErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(AuthorityErrors.NameRequired);

        if (countryId == default)
            throw new DomainException(AuthorityErrors.CountryRequired);

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
