using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Blueprint;

/// <summary>
/// A document a template version expects in one of its sections — typed by a
/// <see cref="DocumentTypeId"/> from the controlled vocabulary, never a file.
/// A child of the aggregate: created only through the template, and it points
/// up at its <see cref="TemplateSection"/>; the section never reaches down to it.
/// </summary>
public sealed class RequiredDocument
{
    internal RequiredDocument(
        RequiredDocumentId id,
        TemplateSectionId sectionId,
        DocumentTypeId documentTypeId,
        bool isMandatory,
        int order)
    {
        if (documentTypeId == default)
            throw new DomainException(
                RegulatoryTemplateErrors.RequiredDocumentTypeRequired);

        Id = id;
        SectionId = sectionId;
        DocumentTypeId = documentTypeId;
        IsMandatory = isMandatory;
        Order = order;
    }

    public RequiredDocumentId Id { get; }

    // The section this document belongs to — the section↔document relationship
    // lives here, on the placeholder, not on the section.
    public TemplateSectionId SectionId { get; }

    // Which *kind* of document ("Cover Letter"), from the DocumentType vocabulary.
    public DocumentTypeId DocumentTypeId { get; }

    // Required vs optional. Cardinality (min/max copies) and conditionality are
    // deferred — this is the thin version of "how much is expected".
    public bool IsMandatory { get; }

    public int Order { get; }
}
