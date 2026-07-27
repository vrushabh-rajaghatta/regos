using FluentAssertions;

using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Tests.Blueprint;

public class TemplateSectionTests
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
    public void AddSection_WithNoDraftVersion_Throws()
    {
        var template = NewTemplate();

        var act = () => template.AddSection("M1", "Administrative Information");

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.NoDraftVersion);
    }

    [Fact]
    public void AddSection_AddsTopLevelSectionToTheDraft()
    {
        var template = NewDraftTemplate();

        var section = template.AddSection("M1", "Administrative Information", null, 1);

        section.Code.Should().Be("M1");
        section.Title.Should().Be("Administrative Information");
        section.ParentSectionId.Should().BeNull();
        section.Order.Should().Be(1);
        template.Versions.Single().Sections.Should().ContainSingle()
            .Which.Should().Be(section);
    }

    [Fact]
    public void AddSection_AddsChildUnderItsParent()
    {
        var template = NewDraftTemplate();
        var m3 = template.AddSection("M3", "Quality", null, 3);

        var substance = template.AddSection("3.2.S", "Drug Substance", m3.Id, 1);

        substance.ParentSectionId.Should().Be(m3.Id);
        template.Versions.Single().Sections.Should().HaveCount(2);
    }

    [Fact]
    public void AddSection_PreservesCtdCasing()
    {
        var template = NewDraftTemplate();

        var section = template.AddSection("3.2.S", "Drug Substance");

        section.Code.Should().Be("3.2.S");
    }

    [Fact]
    public void AddSection_DuplicateCode_Throws()
    {
        var template = NewDraftTemplate();
        template.AddSection("M1", "Administrative Information");

        var act = () => template.AddSection("M1", "Something Else");

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.DuplicateSectionCode);
    }

    [Fact]
    public void AddSection_DuplicateCodeDifferentCasing_Throws()
    {
        var template = NewDraftTemplate();
        template.AddSection("M1", "Administrative Information");

        var act = () => template.AddSection("m1", "Lowercased");

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.DuplicateSectionCode);
    }

    [Fact]
    public void AddSection_UnknownParent_Throws()
    {
        var template = NewDraftTemplate();

        var act = () => template.AddSection(
            "3.2.S", "Drug Substance", TemplateSectionId.New(), 1);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.ParentSectionNotFound);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddSection_BlankCode_Throws(string code)
    {
        var template = NewDraftTemplate();

        var act = () => template.AddSection(code, "A Title");

        act.Should().Throw<DomainException>()
            .WithMessage(RegulatoryTemplateErrors.SectionCodeRequired);
    }

    [Fact]
    public void AddSection_BlankTitle_Throws()
    {
        var template = NewDraftTemplate();

        var act = () => template.AddSection("M1", "  ");

        act.Should().Throw<DomainException>()
            .WithMessage(RegulatoryTemplateErrors.SectionTitleRequired);
    }

    [Fact]
    public void AddSection_AfterPublish_IsRejected()
    {
        var template = NewDraftTemplate();
        var version = template.Versions.Single();
        template.PublishVersion(version.Id, null, DateTime.UtcNow);

        var act = () => template.AddSection("M1", "Administrative Information");

        // No open draft once published — the structure is frozen.
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.NoDraftVersion);
    }
}
