namespace RegOS.ReferenceData.Application.Queries.ApplicationTypes.ListApplicationTypes;

public sealed record ApplicationTypeDto(
    Guid Id,
    string Code,
    string Name,
    Guid AuthorityId);
