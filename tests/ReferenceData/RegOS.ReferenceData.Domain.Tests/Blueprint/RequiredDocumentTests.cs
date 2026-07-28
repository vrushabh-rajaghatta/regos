using FluentAssertions;

using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Tests.Blueprint;

public class RequiredDocumentTests
{
    private static RegulatoryTemplate NewTemplate() =>
        RegulatoryTemplate.Create(
            "FDA_IND_CTD",
            "FDA IND (CTD)",
            new AuthorityId(Guid.NewGuid()),
            new SubmissionTypeId(Guid.NewGuid()),
            "ICH eCTD");

    private static RegulatoryTemplate NewDraftTemplate()
    {
        var template = NewTemplate();
        template.StartDraftVersion();
        return template;
    }

    [Fact]
    public void AddRequiredDocument_WithNoDraftVersion_Throws()
    {
        var template = NewTemplate();

        var act = () => template.AddRequiredDocument(
            TemplateSectionId.New(), DocumentTypeId.New());

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.NoDraftVersion);
    }

    [Fact]
    public void AddRequiredDocument_AttachesToSection()
    {
        var template = NewDraftTemplate();
        var m1 = template.AddSection("M1", "Administrative Information", null, 1);
        var docType = DocumentTypeId.New();

        var required = template.AddRequiredDocument(m1.Id, docType, true, 1);

        required.SectionId.Should().Be(m1.Id);
        required.DocumentTypeId.Should().Be(docType);
        required.IsMandatory.Should().BeTrue();
        required.Order.Should().Be(1);
        template.Versions.Single().RequiredDocuments.Should().ContainSingle()
            .Which.Should().Be(required);
    }

    [Fact]
    public void AddRequiredDocument_DefaultsToMandatory()
    {
        var template = NewDraftTemplate();
        var m1 = template.AddSection("M1", "Administrative Information", null, 1);

        var required = template.AddRequiredDocument(m1.Id, DocumentTypeId.New());

        required.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void AddRequiredDocument_CanBeOptional()
    {
        var template = NewDraftTemplate();
        var m1 = template.AddSection("M1", "Administrative Information", null, 1);

        var required = template.AddRequiredDocument(
            m1.Id, DocumentTypeId.New(), isMandatory: false);

        required.IsMandatory.Should().BeFalse();
    }

    [Fact]
    public void AddRequiredDocument_UnknownSection_Throws()
    {
        var template = NewDraftTemplate();
        template.AddSection("M1", "Administrative Information", null, 1);

        var act = () => template.AddRequiredDocument(
            TemplateSectionId.New(), DocumentTypeId.New());

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.RequiredDocumentSectionNotFound);
    }

    [Fact]
    public void AddRequiredDocument_BlankDocumentType_Throws()
    {
        var template = NewDraftTemplate();
        var m1 = template.AddSection("M1", "Administrative Information", null, 1);

        var act = () => template.AddRequiredDocument(m1.Id, default);

        act.Should().Throw<DomainException>()
            .WithMessage(RegulatoryTemplateErrors.RequiredDocumentTypeRequired);
    }

    [Fact]
    public void AddRequiredDocument_DuplicateTypeInSameSection_Throws()
    {
        var template = NewDraftTemplate();
        var m1 = template.AddSection("M1", "Administrative Information", null, 1);
        var docType = DocumentTypeId.New();
        template.AddRequiredDocument(m1.Id, docType);

        var act = () => template.AddRequiredDocument(m1.Id, docType);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.DuplicateRequiredDocument);
    }

    [Fact]
    public void AddRequiredDocument_SameTypeDifferentSections_IsAllowed()
    {
        var template = NewDraftTemplate();
        var m1 = template.AddSection("M1", "Administrative Information", null, 1);
        var m2 = template.AddSection("M2", "Summaries", null, 2);
        var docType = DocumentTypeId.New();

        template.AddRequiredDocument(m1.Id, docType);
        template.AddRequiredDocument(m2.Id, docType);

        template.Versions.Single().RequiredDocuments.Should().HaveCount(2);
    }

    [Fact]
    public void AddRequiredDocument_AfterPublish_IsRejected()
    {
        var template = NewDraftTemplate();
        var m1 = template.AddSection("M1", "Administrative Information", null, 1);
        var version = template.Versions.Single();
        template.PublishVersion(version.Id, null, DateTime.UtcNow);

        var act = () => template.AddRequiredDocument(m1.Id, DocumentTypeId.New());

        // No open draft once published — the blueprint is frozen.
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.NoDraftVersion);
    }
}
