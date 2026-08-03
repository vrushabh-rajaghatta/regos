using FluentAssertions;

using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Domain.Tests.Submission;

/// <summary>
/// EPIC-019 S002 — which study a placement reports.
/// </summary>
/// <remarks>
/// The fact belongs to the <em>placement</em>, not to the document and not to
/// the study (ADR-053, ADR-056 §4). Two consequences are tested here rather
/// than asserted in prose: a placement reports at most one study, and a
/// document that sits nowhere reports nothing.
/// </remarks>
public class SubmissionReportedStudyTests
{
    private static SubmissionAggregate NewDraft() =>
        SubmissionAggregate.Create(
            TenantId.New(),
            new RegulatoryApplicationId(Guid.NewGuid()),
            "Initial IND",
            SubmissionFormat.Ectd,
            SubmissionClassifications.Any());

    private static SubmissionDocument Placed(SubmissionAggregate submission) =>
        submission.AttachDocument(
            ProductDocumentId.New(),
            DocumentVersionId.New(),
            TemplateSectionId.New());

    [Fact]
    public void APlacement_ReportsANonClinicalStudy()
    {
        var submission = NewDraft();
        var placement = Placed(submission);
        var study = NonClinicalStudyId.New();

        submission.ReportNonClinicalStudy(placement.Id, study);

        placement.NonClinicalStudyId.Should().Be(study);
        placement.ClinicalStudyId.Should().BeNull();
        placement.ReportsAStudy.Should().BeTrue();
    }

    [Fact]
    public void APlacement_ReportsAClinicalStudy()
    {
        var submission = NewDraft();
        var placement = Placed(submission);
        var study = ClinicalStudyId.New();

        submission.ReportClinicalStudy(placement.Id, study);

        placement.ClinicalStudyId.Should().Be(study);
        placement.NonClinicalStudyId.Should().BeNull();
    }

    /// <summary>
    /// The exclusive-or ADR-056 named as S002's to model. It is structural
    /// rather than checked: each writer clears the other, so there is no
    /// sequence of calls that produces a placement reporting two studies.
    /// </summary>
    [Fact]
    public void APlacement_ReportsAtMostOneStudy_WhicheverOrderTheyArrive()
    {
        var submission = NewDraft();
        var placement = Placed(submission);

        submission.ReportNonClinicalStudy(
            placement.Id, NonClinicalStudyId.New());
        submission.ReportClinicalStudy(placement.Id, ClinicalStudyId.New());

        placement.NonClinicalStudyId.Should().BeNull(
            "naming a clinical study replaces whatever was there — a document "
            + "reports one study, and the STF has no way to show two");
        placement.ClinicalStudyId.Should().NotBeNull();

        submission.ReportNonClinicalStudy(
            placement.Id, NonClinicalStudyId.New());

        placement.ClinicalStudyId.Should().BeNull();
        placement.NonClinicalStudyId.Should().NotBeNull();
    }

    [Fact]
    public void AStudyCanBeTakenBackOff_WithoutUnplacingTheDocument()
    {
        var submission = NewDraft();
        var placement = Placed(submission);

        submission.ReportClinicalStudy(placement.Id, ClinicalStudyId.New());
        submission.ClearReportedStudy(placement.Id);

        placement.ReportsAStudy.Should().BeFalse();
        placement.TemplateSectionId.Should().NotBeNull(
            "reporting no study is not the same as being nowhere");
    }

    /// <summary>
    /// The half that makes "a fact about the placement" true of the data and
    /// not only of the comment.
    /// </summary>
    [Fact]
    public void AnUnplacedDocument_CannotReportAStudy()
    {
        var submission = NewDraft();

        var attachment = submission.AttachDocument(
            ProductDocumentId.New(), DocumentVersionId.New());

        var report = () => submission.ReportNonClinicalStudy(
            attachment.Id, NonClinicalStudyId.New());

        report.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*place it in a section first*");
    }

    [Fact]
    public void TakingADocumentOutOfTheDossier_TakesItsStudyWithIt()
    {
        var submission = NewDraft();
        var placement = Placed(submission);

        submission.ReportNonClinicalStudy(
            placement.Id, NonClinicalStudyId.New());

        submission.ClearPlacement(placement.Id);

        placement.ReportsAStudy.Should().BeFalse(
            "a document that sits nowhere reports nothing — leaving the "
            + "reference behind would outlive the placement it describes");
    }

    [Fact]
    public void MovingADocumentToAnotherSection_KeepsTheStudyItReports()
    {
        var submission = NewDraft();
        var placement = Placed(submission);
        var study = NonClinicalStudyId.New();

        submission.ReportNonClinicalStudy(placement.Id, study);
        submission.PlaceDocument(placement.Id, TemplateSectionId.New());

        placement.NonClinicalStudyId.Should().Be(study,
            "the same study can be reported from 4.2.1 or 4.2.3 — moving a "
            + "document is not a statement about which study it reports");
    }

    [Fact]
    public void APublishedSubmission_IsClosedToStudyChanges()
    {
        var submission = NewDraft();
        var placement = Placed(submission);

        submission.Publish(0, null, [], DateTimeOffset.UtcNow);

        var report = () => submission.ReportNonClinicalStudy(
            placement.Id, NonClinicalStudyId.New());

        report.Should().Throw<BusinessRuleViolationException>();
    }
}
