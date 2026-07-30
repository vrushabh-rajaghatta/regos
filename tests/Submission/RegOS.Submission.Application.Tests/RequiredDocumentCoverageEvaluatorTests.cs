using FluentAssertions;

using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.Submission.Application.Validation;
using RegOS.Submission.Application.Validation.Models;
using RegOS.Submission.Application.Validation.Rules;

using IssueSeverity = RegOS.Submission.Application.Validation.Models.ValidationSeverity;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// Pure unit tests — no database. Coverage is a function of the blueprint's
/// placeholders and where the submission's documents actually sit.
/// </summary>
/// <remarks>
/// These fixtures express blueprints the seeded templates cannot: the same
/// document type required by two different sections. That case was the concrete
/// limit ADR-035 recorded and this story retires, so it is tested here rather
/// than waiting for a blueprint that happens to contain it.
/// </remarks>
public class RequiredDocumentCoverageEvaluatorTests
{
    private static readonly DocumentTypeId CoverLetter =
        new(Guid.Parse("50000000-0000-0000-0000-000000000009"));
    private static readonly DocumentTypeId Protocol =
        new(Guid.Parse("50000000-0000-0000-0000-000000000010"));

    [Fact]
    public void APlaceholderFilledInItsOwnSection_IsSatisfied()
    {
        var b = new Blueprint();
        var m1 = b.Section("1.1", "Forms");
        b.Requires(m1, CoverLetter);

        var result = b.Evaluate(Doc(CoverLetter, m1));

        result.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// The whole point of the story: attachment is no longer completeness.
    /// Under EPIC-002's type-only matching this document counted.
    /// </summary>
    [Fact]
    public void ADocumentOfTheRightTypeThatIsNotPlaced_SatisfiesNothing()
    {
        var b = new Blueprint();
        var m1 = b.Section("1.1", "Forms");
        b.Requires(m1, CoverLetter);

        var result = b.Evaluate(Doc(CoverLetter, section: null));

        result.Issues.Should().ContainSingle()
            .Which.Code.Should().Be(SubmissionValidationCodes.RequiredDocumentMissing);
    }

    [Fact]
    public void ADocumentPlacedInTheWrongSection_SatisfiesNothing()
    {
        var b = new Blueprint();
        var m1 = b.Section("1.1", "Forms");
        var m2 = b.Section("1.2", "Cover Letter");
        b.Requires(m1, CoverLetter);

        var result = b.Evaluate(Doc(CoverLetter, m2));

        result.Issues.Should().ContainSingle();
    }

    /// <summary>
    /// No ancestor/descendant inference: a regulator's blueprint names the leaf
    /// it expects the document in, and "close enough" completeness would be
    /// worse than no check.
    /// </summary>
    [Fact]
    public void APlacementInTheParentSection_DoesNotSatisfyTheChild()
    {
        var b = new Blueprint();
        var parent = b.Section("3.2.S", "Drug Substance");
        var child = b.Section("3.2.S.1", "General Information", parent);
        b.Requires(child, CoverLetter);

        var result = b.Evaluate(Doc(CoverLetter, parent));

        result.Issues.Should().ContainSingle();
    }

    /// <summary>
    /// The limit ADR-035 named: "a type required by two sections is satisfied by
    /// one attachment". It now owes two documents.
    /// </summary>
    [Fact]
    public void ATypeRequiredInTwoSections_NeedsAPlacementInEach()
    {
        var b = new Blueprint();
        var first = b.Section("1.1", "Forms");
        var second = b.Section("2.1", "Overviews");
        b.Requires(first, CoverLetter);
        b.Requires(second, CoverLetter);

        var oneOfThem = b.Evaluate(Doc(CoverLetter, first));

        oneOfThem.Issues.Should().ContainSingle()
            .Which.Message.Should().Contain("2.1 Overviews");

        var bothOfThem = b.Evaluate(
            Doc(CoverLetter, first), Doc(CoverLetter, second));

        bothOfThem.Issues.Should().BeEmpty();
    }

    [Fact]
    public void TheIssueNamesTheDocumentTypeAndTheSection()
    {
        var b = new Blueprint();
        var m1 = b.Section("1.1", "Forms");
        b.Requires(m1, CoverLetter);

        var issue = b.Evaluate().Issues.Should().ContainSingle().Subject;

        issue.Message.Should().Contain("Cover Letter").And.Contain("1.1 Forms");
        issue.Severity.Should().Be(IssueSeverity.Error);
    }

    [Fact]
    public void OptionalPlaceholders_AreNotReported()
    {
        var b = new Blueprint();
        var m1 = b.Section("1.1", "Forms");
        b.Requires(m1, CoverLetter, isMandatory: false);

        b.Evaluate().Issues.Should().BeEmpty();
    }

    [Fact]
    public void ADifferentTypeInTheRightSection_DoesNotSatisfyThePlaceholder()
    {
        var b = new Blueprint();
        var m1 = b.Section("1.1", "Forms");
        b.Requires(m1, CoverLetter);

        b.Evaluate(Doc(Protocol, m1)).Issues.Should().ContainSingle();
    }

    // --- fixtures ------------------------------------------------------------

    private static (DocumentTypeId Type, TemplateSectionId? Section) Doc(
        DocumentTypeId type, TemplateSectionId? section) => (type, section);

    /// <summary>
    /// Built through the real aggregate: the child constructors are internal to
    /// the domain, and going through the template is also what guarantees the
    /// fixture is a blueprint the domain would actually allow.
    /// </summary>
    private sealed class Blueprint
    {
        private readonly RegulatoryTemplate _template = RegulatoryTemplate.Create(
            "TEST_COVERAGE",
            "Coverage Test Template",
            new AuthorityId(Guid.NewGuid()),
            new SubmissionTypeId(Guid.NewGuid()),
            "Test");

        private readonly Dictionary<DocumentTypeId, string> _names = new()
        {
            [CoverLetter] = "Cover Letter",
            [Protocol] = "Protocol",
        };

        private int _order;

        public Blueprint() => _template.StartDraftVersion();

        public TemplateSectionId Section(
            string code, string title, TemplateSectionId? parent = null) =>
            _template.AddSection(code, title, parent, ++_order).Id;

        public void Requires(
            TemplateSectionId section,
            DocumentTypeId type,
            bool isMandatory = true) =>
            _template.AddRequiredDocument(section, type, isMandatory, ++_order);

        public SubmissionValidationResult Evaluate(
            params (DocumentTypeId Type, TemplateSectionId? Section)[] documents)
        {
            var context = new BlueprintEvaluationContext(
                _template.Versions.Single(),
                [.. documents.Select(d => new AttachedDocument(
                    d.Type, "doc.pdf", "application/pdf", d.Section))],
                _names);

            var result = new SubmissionValidationResult();
            new RequiredDocumentCoverageEvaluator().Evaluate(context, result);

            return result;
        }
    }
}
