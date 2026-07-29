using FluentAssertions;

using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Tests.Blueprint;

public class ValidationRuleTests
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
    public void AddValidationRule_WithNoDraftVersion_Throws()
    {
        var template = NewTemplate();

        var act = () => template.AddValidationRule(
            "R1", ValidationRuleType.FileFormat, ValidationSeverity.Error, "PDF only");

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.NoDraftVersion);
    }

    [Fact]
    public void AddValidationRule_AddsVersionScopedRule()
    {
        var template = NewDraftTemplate();

        var rule = template.AddValidationRule(
            "FDA-IND-PDF",
            ValidationRuleType.FileFormat,
            ValidationSeverity.Error,
            "All documents must be PDF.",
            parameters: "pdf",
            order: 1);

        rule.Code.Should().Be("FDA-IND-PDF");
        rule.RuleType.Should().Be(ValidationRuleType.FileFormat);
        rule.Severity.Should().Be(ValidationSeverity.Error);
        rule.SectionId.Should().BeNull();
        rule.Parameters.Should().Be("pdf");
        rule.Order.Should().Be(1);
        template.Versions.Single().ValidationRules.Should().ContainSingle()
            .Which.Should().Be(rule);
    }

    [Fact]
    public void AddValidationRule_AddsSectionScopedRule()
    {
        var template = NewDraftTemplate();
        var m1 = template.AddSection("M1", "Administrative Information", null, 1);

        var rule = template.AddValidationRule(
            "M1-NONEMPTY",
            ValidationRuleType.SectionNotEmpty,
            ValidationSeverity.Warning,
            "Module 1 should not be empty.",
            sectionId: m1.Id);

        rule.SectionId.Should().Be(m1.Id);
        rule.Severity.Should().Be(ValidationSeverity.Warning);
    }

    [Fact]
    public void AddValidationRule_PreservesCodeCasing()
    {
        var template = NewDraftTemplate();

        var rule = template.AddValidationRule(
            "FDA-IND-PDF", ValidationRuleType.FileFormat,
            ValidationSeverity.Error, "PDF only");

        rule.Code.Should().Be("FDA-IND-PDF");
    }

    [Fact]
    public void AddValidationRule_BlankParameters_BecomeNull()
    {
        var template = NewDraftTemplate();

        var rule = template.AddValidationRule(
            "R1", ValidationRuleType.SectionNotEmpty,
            ValidationSeverity.Error, "Not empty", parameters: "   ");

        rule.Parameters.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddValidationRule_BlankCode_Throws(string code)
    {
        var template = NewDraftTemplate();

        var act = () => template.AddValidationRule(
            code, ValidationRuleType.FileFormat, ValidationSeverity.Error, "PDF only");

        act.Should().Throw<DomainException>()
            .WithMessage(RegulatoryTemplateErrors.ValidationRuleCodeRequired);
    }

    [Fact]
    public void AddValidationRule_BlankMessage_Throws()
    {
        var template = NewDraftTemplate();

        var act = () => template.AddValidationRule(
            "R1", ValidationRuleType.FileFormat, ValidationSeverity.Error, "  ");

        act.Should().Throw<DomainException>()
            .WithMessage(RegulatoryTemplateErrors.ValidationRuleMessageRequired);
    }

    [Fact]
    public void AddValidationRule_DuplicateCode_Throws()
    {
        var template = NewDraftTemplate();
        template.AddValidationRule(
            "R1", ValidationRuleType.FileFormat, ValidationSeverity.Error, "PDF only");

        var act = () => template.AddValidationRule(
            "r1", ValidationRuleType.SectionNotEmpty, ValidationSeverity.Warning, "Not empty");

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.DuplicateValidationRuleCode);
    }

    [Fact]
    public void AddValidationRule_UnknownSection_Throws()
    {
        var template = NewDraftTemplate();

        var act = () => template.AddValidationRule(
            "R1", ValidationRuleType.SectionNotEmpty, ValidationSeverity.Error,
            "Not empty", sectionId: TemplateSectionId.New());

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.ValidationRuleSectionNotFound);
    }

    [Fact]
    public void AddValidationRule_AfterPublish_IsRejected()
    {
        var template = NewDraftTemplate();
        var version = template.Versions.Single();
        template.PublishVersion(version.Id, null, DateTime.UtcNow);

        var act = () => template.AddValidationRule(
            "R1", ValidationRuleType.FileFormat, ValidationSeverity.Error, "PDF only");

        // No open draft once published — the blueprint is frozen.
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.NoDraftVersion);
    }
}
