using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Blueprint;

/// <summary>
/// One version of a dossier blueprint. Created as a <see cref="TemplateVersionStatus.Draft"/>
/// by its owning <see cref="RegulatoryTemplate"/> and frozen on
/// <see cref="Publish"/>. Its sections, required documents and validation rules
/// (later stories) will hang off this version and inherit its immutability.
/// </summary>
public sealed class RegulatoryTemplateVersion
{
    private readonly List<TemplateSection> _sections = [];
    private readonly List<RequiredDocument> _requiredDocuments = [];
    private readonly List<ValidationRule> _validationRules = [];

    // Internal so only the RegulatoryTemplate aggregate (same assembly) can
    // create a version — there is no path to instantiate one independently of
    // its root.
    internal RegulatoryTemplateVersion(
        RegulatoryTemplateVersionId id,
        int versionNumber)
    {
        Id = id;
        VersionNumber = versionNumber;
        Status = TemplateVersionStatus.Draft;
    }

    public RegulatoryTemplateVersionId Id { get; }

    public int VersionNumber { get; }

    public TemplateVersionStatus Status { get; private set; }

    // Temporal validity — the governance seam. Set at publish; the query
    // logic that uses it ("which version applied on date X") is deferred.
    public DateOnly? EffectiveFrom { get; private set; }

    public DateOnly? EffectiveTo { get; private set; }

    public DateTime? PublishedOnUtc { get; private set; }

    // Section management stays inside the aggregate — never expose a mutable list.
    public IReadOnlyCollection<TemplateSection> Sections => _sections.AsReadOnly();

    // Required documents hang off the version (flat), each pointing at its section.
    public IReadOnlyCollection<RequiredDocument> RequiredDocuments
        => _requiredDocuments.AsReadOnly();

    // Validation rules hang off the version (flat); each optionally targets a section.
    public IReadOnlyCollection<ValidationRule> ValidationRules
        => _validationRules.AsReadOnly();

    internal void Publish(DateOnly? effectiveFrom, DateTime publishedOnUtc)
    {
        if (Status == TemplateVersionStatus.Published)
            throw new BusinessRuleViolationException(
                RegulatoryTemplateErrors.VersionAlreadyPublished);

        Status = TemplateVersionStatus.Published;
        EffectiveFrom = effectiveFrom;
        PublishedOnUtc = publishedOnUtc;
    }

    internal TemplateSection AddSection(
        string code,
        string title,
        TemplateSectionId? parentSectionId,
        int order)
    {
        if (Status != TemplateVersionStatus.Draft)
            throw new BusinessRuleViolationException(
                RegulatoryTemplateErrors.VersionNotDraft);

        // Construct first — the entity validates code/title format.
        var section = new TemplateSection(
            TemplateSectionId.New(), code, title, parentSectionId, order);

        if (_sections.Any(s => string.Equals(
                s.Code, section.Code, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessRuleViolationException(
                RegulatoryTemplateErrors.DuplicateSectionCode);

        if (section.ParentSectionId is { } parentId
            && _sections.All(s => s.Id != parentId))
            throw new BusinessRuleViolationException(
                RegulatoryTemplateErrors.ParentSectionNotFound);

        _sections.Add(section);

        return section;
    }

    internal RequiredDocument AddRequiredDocument(
        TemplateSectionId sectionId,
        DocumentTypeId documentTypeId,
        bool isMandatory,
        int order)
    {
        if (Status != TemplateVersionStatus.Draft)
            throw new BusinessRuleViolationException(
                RegulatoryTemplateErrors.VersionNotDraft);

        // The document must attach to a section that lives in this version.
        if (_sections.All(s => s.Id != sectionId))
            throw new BusinessRuleViolationException(
                RegulatoryTemplateErrors.RequiredDocumentSectionNotFound);

        // One requirement per (section, document type): presence, not cardinality.
        if (_requiredDocuments.Any(
                d => d.SectionId == sectionId && d.DocumentTypeId == documentTypeId))
            throw new BusinessRuleViolationException(
                RegulatoryTemplateErrors.DuplicateRequiredDocument);

        // Construct after the section check — the entity validates the type id.
        var document = new RequiredDocument(
            RequiredDocumentId.New(), sectionId, documentTypeId, isMandatory, order);

        _requiredDocuments.Add(document);

        return document;
    }

    internal ValidationRule AddValidationRule(
        string code,
        ValidationRuleType ruleType,
        ValidationSeverity severity,
        string message,
        TemplateSectionId? sectionId,
        string? parameters,
        int order)
    {
        if (Status != TemplateVersionStatus.Draft)
            throw new BusinessRuleViolationException(
                RegulatoryTemplateErrors.VersionNotDraft);

        // A section-scoped rule must target a section that lives in this version.
        if (sectionId is { } target && _sections.All(s => s.Id != target))
            throw new BusinessRuleViolationException(
                RegulatoryTemplateErrors.ValidationRuleSectionNotFound);

        // Construct first — the entity validates code/message format.
        var rule = new ValidationRule(
            ValidationRuleId.New(), code, ruleType, severity,
            message, sectionId, parameters, order);

        if (_validationRules.Any(r => string.Equals(
                r.Code, rule.Code, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessRuleViolationException(
                RegulatoryTemplateErrors.DuplicateValidationRuleCode);

        _validationRules.Add(rule);

        return rule;
    }
}
