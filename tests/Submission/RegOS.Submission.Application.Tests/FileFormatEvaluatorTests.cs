using FluentAssertions;

using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.Submission.Application.Validation;
using RegOS.Submission.Application.Validation.Models;
using RegOS.Submission.Application.Validation.Rules;

using BlueprintSeverity = RegOS.ReferenceData.Domain.Blueprint.ValidationSeverity;
using IssueSeverity = RegOS.Submission.Application.Validation.Models.ValidationSeverity;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// Pure unit tests — no database. The evaluator is a function of the rule and
/// the gathered context, which is what passing state instead of a DbContext
/// buys.
/// </summary>
public class FileFormatEvaluatorTests
{
    private static readonly DocumentTypeId AnyType =
        new(Guid.Parse("50000000-0000-0000-0000-000000000009"));

    // --- CanEvaluate ---------------------------------------------------------

    [Fact]
    public void Evaluates_VersionWideFileFormatRules()
    {
        var (_, rule) = Build(ValidationRuleType.FileFormat, "pdf");

        new FileFormatEvaluator().CanEvaluate(rule).Should().BeTrue();
    }

    [Fact]
    public void DoesNotEvaluate_OtherRuleTypes()
    {
        var (_, rule) = Build(ValidationRuleType.SectionNotEmpty, null);

        new FileFormatEvaluator().CanEvaluate(rule).Should().BeFalse();
    }

    [Fact]
    public void DoesNotEvaluate_SectionScopedRules()
    {
        // "Which documents belong to this section?" needs placement, which does
        // not exist yet — so the rule is disclosed as unevaluated, not applied
        // to the whole dossier as if it were version-wide.
        var (_, rule) = Build(ValidationRuleType.FileFormat, "pdf", sectionScoped: true);

        new FileFormatEvaluator().CanEvaluate(rule).Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DoesNotEvaluate_RulesWithNoAcceptedFormats(string? parameters)
    {
        // States nothing to check; disclosed rather than vacuously passed.
        var (_, rule) = Build(ValidationRuleType.FileFormat, parameters);

        new FileFormatEvaluator().CanEvaluate(rule).Should().BeFalse();
    }

    // --- Evaluate ------------------------------------------------------------

    [Fact]
    public void AcceptedFormat_ProducesNoIssue()
    {
        var result = Evaluate("pdf", Doc("protocol.pdf", "application/pdf"));

        result.Issues.Should().BeEmpty();
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void WrongFormat_IsReportedWithTheRulesOwnCode()
    {
        var result = Evaluate("pdf", Doc("protocol.docx", "application/vnd.word"));

        var issue = result.Issues.Should().ContainSingle().Subject;
        issue.Code.Should().Be(SubmissionValidationCodes.BlueprintRuleViolation);
        issue.RuleCode.Should().Be("TEST-PDF");
        issue.Severity.Should().Be(IssueSeverity.Error);
        issue.Message.Should().Contain("protocol.docx").And.Contain("docx");
    }

    [Fact]
    public void ExtensionWins_OverAMisleadingContentType()
    {
        // Content types are assigned by whichever client uploaded the file.
        var result = Evaluate("pdf", Doc("protocol.docx", "application/pdf"));

        result.Issues.Should().ContainSingle();
    }

    [Fact]
    public void ContentType_IsUsedWhenThereIsNoExtension()
    {
        var result = Evaluate("pdf", Doc("protocol", "application/pdf"));

        result.Issues.Should().BeEmpty();
    }

    [Fact]
    public void ContentTypeParameters_AreIgnored()
    {
        var result = Evaluate("pdf", Doc("protocol", "application/pdf; charset=utf-8"));

        result.Issues.Should().BeEmpty();
    }

    [Fact]
    public void UndeterminableFormat_FailsClosed()
    {
        // Inability to establish compliance is not compliance.
        var result = Evaluate("pdf", Doc("protocol", ""));

        var issue = result.Issues.Should().ContainSingle().Subject;
        issue.Severity.Should().Be(IssueSeverity.Error);
        issue.Message.Should().Contain("could not be determined");
    }

    [Theory]
    [InlineData("pdf,docx")]
    [InlineData("PDF, .DOCX")]
    public void MultipleAcceptedFormats_AreParsedAndNormalised(string parameters)
    {
        var result = Evaluate(
            parameters, Doc("a.pdf", "application/pdf"), Doc("b.docx", "x"));

        result.Issues.Should().BeEmpty();
    }

    [Fact]
    public void EveryOffendingDocument_IsReported()
    {
        var result = Evaluate(
            "pdf",
            Doc("a.pdf", "application/pdf"),
            Doc("b.docx", "x"),
            Doc("c.xlsx", "x"));

        result.Issues.Should().HaveCount(2);
    }

    [Fact]
    public void WarningRules_DoNotBlockPublishing()
    {
        var result = Evaluate(
            "pdf", BlueprintSeverity.Warning, Doc("protocol.docx", "x"));

        result.Issues.Should().ContainSingle()
            .Which.Severity.Should().Be(IssueSeverity.Warning);
        result.IsValid.Should().BeTrue();
    }

    // --- helpers -------------------------------------------------------------

    private static AttachedDocument Doc(string fileName, string contentType) =>
        new(AnyType, fileName, contentType);

    private static SubmissionValidationResult Evaluate(
        string? parameters, params AttachedDocument[] documents) =>
        Evaluate(parameters, BlueprintSeverity.Error, documents);

    private static SubmissionValidationResult Evaluate(
        string? parameters,
        BlueprintSeverity severity,
        params AttachedDocument[] documents)
    {
        var (version, rule) = Build(
            ValidationRuleType.FileFormat, parameters, severity: severity);

        var context = new BlueprintEvaluationContext(
            version, documents, new Dictionary<DocumentTypeId, string>());

        var result = new SubmissionValidationResult();
        new FileFormatEvaluator().Evaluate(rule, context, result);

        return result;
    }

    /// <summary>
    /// Rules are built through the real aggregate — their constructor is
    /// internal to the domain, and going through the template is also what
    /// guarantees the fixture is a rule the domain would actually allow.
    /// </summary>
    private static (RegulatoryTemplateVersion Version, ValidationRule Rule) Build(
        ValidationRuleType ruleType,
        string? parameters,
        bool sectionScoped = false,
        BlueprintSeverity severity = BlueprintSeverity.Error)
    {
        var template = RegulatoryTemplate.Create(
            "TEST_TEMPLATE",
            "Test Template",
            new AuthorityId(Guid.NewGuid()),
            new SubmissionTypeId(Guid.NewGuid()),
            "Test");

        template.StartDraftVersion();

        TemplateSectionId? sectionId = sectionScoped
            ? template.AddSection("M1", "Administrative Information", null, 1).Id
            : null;

        template.AddValidationRule(
            "TEST-PDF",
            ruleType,
            severity,
            "All submission documents must be provided as PDF.",
            sectionId,
            parameters,
            1);

        var version = template.Versions.Single();

        return (version, version.ValidationRules.Single());
    }
}
