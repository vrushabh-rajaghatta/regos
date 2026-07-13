namespace RegOS.MasterData.Application.Queries.Regulatory.ListAuthorities;

public sealed record AuthorityDto(
    Guid Id,
    string Code,
    string Name,
    Guid CountryId);
