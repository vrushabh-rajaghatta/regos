using FluentAssertions;

using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.Submission.Application.Validation;
using RegOS.Submission.Application.Validation.Models;
using RegOS.Submission.Application.Validation.Rules;

using BlueprintSeverity = RegOS.ReferenceData.Domain.Blueprint.ValidationSeverity;
using IssueSeverity = RegOS.Submission.Application.Validation.Models.ValidationSeverity;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// The rule EPIC-002 could only disclose. Pure unit tests — no database.
/// </summary>
public class SectionNotEmptyEvaluatorTests
{
    private static readonly DocumentTypeId AnyType =
        new(Guid.Parse("50000000-0000-0000-0000-000000000009"));

    // --- CanEvaluate ---------------------------------------------------------

    [Fact]
    public void Evaluates_SectionScopedRulesOfItsOwnType()
    {
        var b = new Blueprint();
        var rule = b.Rule(b.Section("1.1", "Forms"));

        new SectionNotEmptyEvaluator().CanEvaluate(rule).Should().BeTrue();
    }

    [Fact]
    public void DoesNotEvaluate_OtherRuleTypes()
    {
        var b = new Blueprint();
        var rule = b.Rule(b.Section("1.1", "Forms"), ValidationRuleType.FileFormat);

        new SectionNotEmptyEvaluator().CanEvaluate(rule).Should().BeFalse();
    }

    /// <summary>
    /// A rule of this type with no section names nothing to check. Disclosed as
    /// unevaluated rather than vacuously passed — and not silently widened to
    /// "the dossier must not be empty", which is a different rule.
    /// </summary>
    [Fact]
    public void DoesNotEvaluate_ARuleWithNoSection()
    {
        var b = new Blueprint();
        var rule = b.Rule(section: null);

        new SectionNotEmptyEvaluator().CanEvaluate(rule).Should().BeFalse();
    }

    // --- Evaluate ------------------------------------------------------------

    [Fact]
    public void ADocumentInTheSection_SatisfiesTheRule()
    {
        var b = new Blueprint();
        var forms = b.Section("1.1", "Forms");

        b.Evaluate(b.Rule(forms), (AnyType, forms)).Issues.Should().BeEmpty();
    }

    [Fact]
    public void AnEmptySection_IsReportedWithTheRulesOwnCode()
    {
        var b = new Blueprint();
        var forms = b.Section("1.1", "Forms");

        var issue = b.Evaluate(b.Rule(forms)).Issues.Should().ContainSingle().Subject;

        issue.Code.Should().Be(SubmissionValidationCodes.BlueprintRuleViolation);
        issue.RuleCode.Should().Be("TEST-NONEMPTY");
        issue.Severity.Should().Be(IssueSeverity.Error);
        issue.Message.Should().Contain("1.1 Forms");
    }

    /// <summary>
    /// The subtree ruling. An author writing this rule against 3.2.S means
    /// "Drug Substance must contain content" — not "a document must be filed
    /// directly on the parent node", which a well-organised dossier never does.
    /// </summary>
    [Fact]
    public void ADocumentInADescendantSection_SatisfiesARuleOnItsAncestor()
    {
        var b = new Blueprint();
        var substance = b.Section("3.2.S", "Drug Substance");
        var general = b.Section("3.2.S.1", "General Information", substance);
        var deeper = b.Section("3.2.S.1.1", "Nomenclature", general);

        b.Evaluate(b.Rule(substance), (AnyType, deeper)).Issues.Should().BeEmpty();
    }

    [Fact]
    public void ADocumentInAnAncestorSection_DoesNotSatisfyARuleOnItsDescendant()
    {
        var b = new Blueprint();
        var substance = b.Section("3.2.S", "Drug Substance");
        var general = b.Section("3.2.S.1", "General Information", substance);

        b.Evaluate(b.Rule(general), (AnyType, substance)).Issues.Should().ContainSingle();
    }

    [Fact]
    public void ADocumentInASiblingSection_DoesNotSatisfyTheRule()
    {
        var b = new Blueprint();
        var forms = b.Section("1.1", "Forms");
        var cover = b.Section("1.2", "Cover Letter");

        b.Evaluate(b.Rule(forms), (AnyType, cover)).Issues.Should().ContainSingle();
    }

    [Fact]
    public void AnUnplacedDocument_DoesNotFillAnySection()
    {
        var b = new Blueprint();
        var forms = b.Section("1.1", "Forms");

        b.Evaluate(b.Rule(forms), (AnyType, null)).Issues.Should().ContainSingle();
    }

    [Fact]
    public void WarningRules_DoNotBlockPublishing()
    {
        var b = new Blueprint();
        var stability = b.Section("3.2.S.7", "Stability");

        var result = b.Evaluate(b.Rule(stability, severity: BlueprintSeverity.Warning));

        result.Issues.Should().ContainSingle()
            .Which.Severity.Should().Be(IssueSeverity.Warning);
        result.IsValid.Should().BeTrue();
    }

    // --- fixtures ------------------------------------------------------------

    private sealed class Blueprint
    {
        private readonly RegulatoryTemplate _template = RegulatoryTemplate.Create(
            "TEST_NONEMPTY",
            "Section Not Empty Test Template",
            new AuthorityId(Guid.NewGuid()),
            new ApplicationTypeId(Guid.NewGuid()),
            "Test");

        private int _order;

        public Blueprint() => _template.StartDraftVersion();

        public TemplateSectionId Section(
            string code, string title, TemplateSectionId? parent = null) =>
            _template.AddSection(code, title, parent, ++_order).Id;

        public ValidationRule Rule(
            TemplateSectionId? section,
            ValidationRuleType ruleType = ValidationRuleType.SectionNotEmpty,
            BlueprintSeverity severity = BlueprintSeverity.Error) =>
            _template.AddValidationRule(
                "TEST-NONEMPTY",
                ruleType,
                severity,
                "This section must contain documents.",
                section,
                parameters: null,
                order: ++_order);

        public SubmissionValidationResult Evaluate(
            ValidationRule rule,
            params (DocumentTypeId Type, TemplateSectionId? Section)[] documents)
        {
            var context = new BlueprintEvaluationContext(
                _template.Versions.Single(),
                [.. documents.Select(d => new AttachedDocument(
                    d.Type, "doc.pdf", "application/pdf", d.Section))],
                new Dictionary<DocumentTypeId, string>());

            var result = new SubmissionValidationResult();
            new SectionNotEmptyEvaluator().Evaluate(rule, context, result);

            return result;
        }
    }
}
