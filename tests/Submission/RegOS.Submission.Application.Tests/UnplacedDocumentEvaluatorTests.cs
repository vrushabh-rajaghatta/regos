using FluentAssertions;

using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.Submission.Application.Validation;
using RegOS.Submission.Application.Validation.Models;
using RegOS.Submission.Application.Validation.Rules;

using IssueSeverity = RegOS.Submission.Application.Validation.Models.ValidationSeverity;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// The cleanup question, separate from the completeness one: is every attached
/// document actually somewhere?
/// </summary>
public class UnplacedDocumentEvaluatorTests
{
    private static readonly DocumentTypeId AnyType =
        new(Guid.Parse("50000000-0000-0000-0000-000000000009"));

    [Fact]
    public void NothingUnplaced_ProducesNoIssue()
    {
        var result = Evaluate(Placed(), Placed());

        result.Issues.Should().BeEmpty();
    }

    [Fact]
    public void NoDocumentsAtAll_ProducesNoIssue()
    {
        Evaluate().Issues.Should().BeEmpty();
    }

    [Fact]
    public void UnplacedDocuments_AreReportedButDoNotBlockPublishing()
    {
        var result = Evaluate(Unplaced(), Unplaced(), Placed());

        var issue = result.Issues.Should().ContainSingle().Subject;

        issue.Code.Should().Be(SubmissionValidationCodes.DocumentsNotPlaced);
        issue.Severity.Should().Be(IssueSeverity.Information);

        // Untidy, not invalid.
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void OneIssueCoversThemAll_CountedNotNamed()
    {
        var result = Evaluate(Unplaced(), Unplaced(), Unplaced());

        // A count, not a list: the content plan is the authoritative answer to
        // *which* documents, and a message that grows with the dossier is a
        // message nobody can read.
        result.Issues.Should().ContainSingle()
            .Which.Message.Should().StartWith("3 attached documents");
    }

    [Fact]
    public void ASingleUnplacedDocument_ReadsAsOne()
    {
        Evaluate(Unplaced()).Issues.Should().ContainSingle()
            .Which.Message.Should().StartWith("1 attached document has");
    }

    // --- fixtures ------------------------------------------------------------

    private static AttachedDocument Unplaced() =>
        new(AnyType, "doc.pdf", "application/pdf");

    private static AttachedDocument Placed() =>
        new(AnyType, "doc.pdf", "application/pdf", TemplateSectionId.New());

    private static SubmissionValidationResult Evaluate(
        params AttachedDocument[] documents)
    {
        var template = RegulatoryTemplate.Create(
            "TEST_UNPLACED",
            "Unplaced Test Template",
            new AuthorityId(Guid.NewGuid()),
            new ApplicationTypeId(Guid.NewGuid()),
            "Test");

        template.StartDraftVersion();

        var context = new BlueprintEvaluationContext(
            template.Versions.Single(),
            documents,
            new Dictionary<DocumentTypeId, string>());

        var result = new SubmissionValidationResult();
        new UnplacedDocumentEvaluator().Evaluate(context, result);

        return result;
    }
}
