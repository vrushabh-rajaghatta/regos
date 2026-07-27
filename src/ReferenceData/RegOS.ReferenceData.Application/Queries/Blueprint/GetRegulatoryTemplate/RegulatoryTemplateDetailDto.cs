namespace RegOS.ReferenceData.Application.Queries.Blueprint.GetRegulatoryTemplate;

public sealed record RegulatoryTemplateDetailDto(
    Guid Id,
    string Code,
    string Name,
    Guid AuthorityId,
    Guid SubmissionTypeId,
    string Source,
    string Status,
    IReadOnlyList<RegulatoryTemplateVersionDto> Versions);

public sealed record RegulatoryTemplateVersionDto(
    Guid Id,
    int VersionNumber,
    string Status,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    DateTime? PublishedOnUtc,
    IReadOnlyList<TemplateSectionDto> Sections);

public sealed record TemplateSectionDto(
    Guid Id,
    string Code,
    string Title,
    Guid? ParentSectionId,
    int Order);
