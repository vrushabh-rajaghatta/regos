using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Blueprint;

/// <summary>
/// A node in a template version's dossier tree (e.g. CTD "Module 3", "3.2.S").
/// A child of the aggregate: created only through the template, never
/// independently. It knows only its parent section — required documents attach
/// themselves to it later (STORY-004), it never reaches down to them.
/// </summary>
public sealed class TemplateSection
{
    internal TemplateSection(
        TemplateSectionId id,
        string code,
        string title,
        TemplateSectionId? parentSectionId,
        int order)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(RegulatoryTemplateErrors.SectionCodeRequired);

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException(RegulatoryTemplateErrors.SectionTitleRequired);

        Id = id;
        // CTD codes carry meaningful casing ("3.2.S" vs "3.2.s") — trim only.
        Code = code.Trim();
        Title = title.Trim();
        ParentSectionId = parentSectionId;
        Order = order;
    }

    public TemplateSectionId Id { get; }

    public string Code { get; }

    public string Title { get; }

    // null => top-level module; otherwise the parent section in the same version.
    public TemplateSectionId? ParentSectionId { get; }

    public int Order { get; }
}
