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
    IReadOnlyList<TemplateSectionDto> Sections,
    IReadOnlyList<RequiredDocumentDto> RequiredDocuments,
    IReadOnlyList<ValidationRuleDto> ValidationRules);

public sealed record TemplateSectionDto(
    Guid Id,
    string Code,
    string Title,
    Guid? ParentSectionId,
    int Order);

public sealed record RequiredDocumentDto(
    Guid Id,
    Guid SectionId,
    Guid DocumentTypeId,
    bool IsMandatory,
    int Order);

public sealed record ValidationRuleDto(
    Guid Id,
    string Code,
    string RuleType,
    string Severity,
    Guid? SectionId,
    string? Parameters,
    string Message,
    int Order);
