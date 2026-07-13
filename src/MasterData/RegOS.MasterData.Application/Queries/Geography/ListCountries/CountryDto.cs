namespace RegOS.MasterData.Application.Queries.Geography.ListCountries;

public sealed record CountryDto(
    Guid Id,
    string Code,
    string Name);
