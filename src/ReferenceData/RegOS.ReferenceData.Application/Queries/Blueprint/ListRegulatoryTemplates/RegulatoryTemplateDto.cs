namespace RegOS.ReferenceData.Application.Queries.Blueprint.ListRegulatoryTemplates;

public sealed record RegulatoryTemplateDto(
    Guid Id,
    string Code,
    string Name,
    Guid AuthorityId,
    Guid ApplicationTypeId,
    string Source,
    string Status);
