using FluentAssertions;

using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Domain.Tests.Submission;

/// <summary>
/// Placement — where in the dossier an attached document sits.
/// </summary>
/// <remarks>
/// The aggregate enforces only what it can see: the submission is a draft, and
/// the document is already attached to <em>this</em> submission. Whether the
/// section belongs to the bound template version is Reference Data's business
/// and lives in the application layer.
/// </remarks>
public class SubmissionPlacementTests
{
    private static SubmissionAggregate NewDraft() =>
        SubmissionAggregate.Create(
            TenantId.New(),
            new RegulatoryApplicationId(Guid.NewGuid()),
            new SubmissionTypeId(Guid.NewGuid()),
            "Initial IND",
            SubmissionFormat.Ectd);

    private static (ProductDocumentId Doc, DocumentVersionId Version) NewRef() =>
        (ProductDocumentId.New(), DocumentVersionId.New());

    // --- Attaching with a placement -----------------------------------------

    [Fact]
    public void Attach_WithoutSection_LeavesTheDocumentUnplaced()
    {
        var submission = NewDraft();
        var (doc, version) = NewRef();

        var attachment = submission.AttachDocument(doc, version);

        attachment.TemplateSectionId.Should().BeNull();
    }

    [Fact]
    public void Attach_WithSection_PlacesItInOneStep()
    {
        var submission = NewDraft();
        var (doc, version) = NewRef();
        var section = TemplateSectionId.New();

        var attachment = submission.AttachDocument(doc, version, section);

        attachment.TemplateSectionId.Should().Be(section);
    }

    // --- Placing an attached document ---------------------------------------

    [Fact]
    public void Place_SetsTheSection()
    {
        var submission = NewDraft();
        var (doc, version) = NewRef();
        var attachment = submission.AttachDocument(doc, version);
        var section = TemplateSectionId.New();

        submission.PlaceDocument(attachment.Id, section);

        attachment.TemplateSectionId.Should().Be(section);
    }

    [Fact]
    public void Place_MovesADocumentThatIsAlreadyPlaced()
    {
        var submission = NewDraft();
        var (doc, version) = NewRef();
        var attachment = submission.AttachDocument(
            doc, version, TemplateSectionId.New());

        var destination = TemplateSectionId.New();

        submission.PlaceDocument(attachment.Id, destination);

        attachment.TemplateSectionId.Should().Be(destination);
    }

    [Fact]
    public void Place_LeavesOtherDocumentsAlone()
    {
        var submission = NewDraft();
        var first = NewRef();
        var second = NewRef();
        var moved = submission.AttachDocument(first.Doc, first.Version);
        var untouched = submission.AttachDocument(second.Doc, second.Version);

        submission.PlaceDocument(moved.Id, TemplateSectionId.New());

        untouched.TemplateSectionId.Should().BeNull();
    }

    [Fact]
    public void Place_RequiresASection()
    {
        var submission = NewDraft();
        var (doc, version) = NewRef();
        var attachment = submission.AttachDocument(doc, version);

        var place = () => submission.PlaceDocument(attachment.Id, default);

        place.Should().Throw<DomainException>()
            .WithMessage(SubmissionErrors.TemplateSectionRequired);
    }

    /// <summary>
    /// The invariant that keeps placement from becoming a second, unguarded way
    /// to attach: an id this submission does not own is rejected outright.
    /// Accepting it would bypass every rule AttachDocument enforces — product
    /// ownership, active status, version pinning.
    /// </summary>
    [Fact]
    public void Place_RejectsADocumentThatIsNotAttachedToThisSubmission()
    {
        var submission = NewDraft();
        var (doc, version) = NewRef();
        submission.AttachDocument(doc, version);

        var somewhereElse = SubmissionDocumentId.New();

        var place = () => submission.PlaceDocument(
            somewhereElse, TemplateSectionId.New());

        place.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.DocumentNotAttached);

        submission.Documents.Should().ContainSingle(
            "placing an unknown document must not attach it");
    }

    [Fact]
    public void Place_RejectedOncePublished()
    {
        var submission = NewDraft();
        var (doc, version) = NewRef();
        var attachment = submission.AttachDocument(doc, version);
        submission.Publish(0, null, [], DateTimeOffset.UtcNow);

        var place = () => submission.PlaceDocument(
            attachment.Id, TemplateSectionId.New());

        place.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.DocumentsLockedUnlessDraft);
    }

    // --- Clearing a placement ------------------------------------------------

    [Fact]
    public void ClearPlacement_LeavesTheDocumentAttachedButUnplaced()
    {
        var submission = NewDraft();
        var (doc, version) = NewRef();
        var attachment = submission.AttachDocument(
            doc, version, TemplateSectionId.New());

        submission.ClearPlacement(attachment.Id);

        attachment.TemplateSectionId.Should().BeNull();
        submission.Documents.Should().ContainSingle(
            "clearing a placement removes it from the structure, not the dossier");
    }

    [Fact]
    public void ClearPlacement_RejectsADocumentThatIsNotAttached()
    {
        var submission = NewDraft();

        var clear = () => submission.ClearPlacement(SubmissionDocumentId.New());

        clear.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.DocumentNotAttached);
    }

    [Fact]
    public void ClearPlacement_RejectedOncePublished()
    {
        var submission = NewDraft();
        var (doc, version) = NewRef();
        var attachment = submission.AttachDocument(
            doc, version, TemplateSectionId.New());
        submission.Publish(0, null, [], DateTimeOffset.UtcNow);

        var clear = () => submission.ClearPlacement(attachment.Id);

        clear.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.DocumentsLockedUnlessDraft);
    }
}
