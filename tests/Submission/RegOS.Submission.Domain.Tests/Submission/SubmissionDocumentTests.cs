using RegOS.SharedKernel.Primitives;
using FluentAssertions;

using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Submission.Domain.Tests.Submission;

public class SubmissionDocumentTests
{
    private static SubmissionAggregate NewDraft() =>
        SubmissionAggregate.Create(TenantId.New(), 
            new RegulatoryApplicationId(Guid.NewGuid()),
            "Initial 510(k)",
            SubmissionFormat.Ectd,
            SubmissionClassifications.Any());

    private static (ProductDocumentId Doc, DocumentVersionId Version) NewRef() =>
        (ProductDocumentId.New(), DocumentVersionId.New());

    // --- Attach --------------------------------------------------------------

    [Fact]
    public void Attach_FirstDocument_GetsDisplayOrderOne()
    {
        var submission = NewDraft();
        var (doc, version) = NewRef();

        submission.AttachDocument(doc, version);

        submission.Documents.Should().ContainSingle()
            .Which.DisplayOrder.Should().Be(1);
    }

    [Fact]
    public void Attach_SecondDocument_GetsDisplayOrderTwo()
    {
        var submission = NewDraft();
        var first = NewRef();
        var second = NewRef();

        submission.AttachDocument(first.Doc, first.Version);
        submission.AttachDocument(second.Doc, second.Version);

        submission.Documents.Should().HaveCount(2);
        submission.Documents
            .Single(d => d.ProductDocumentId == second.Doc)
            .DisplayOrder.Should().Be(2);
    }

    [Fact]
    public void Attach_PopulatesReferenceAndAttachedOn()
    {
        var submission = NewDraft();
        var (doc, version) = NewRef();

        submission.AttachDocument(doc, version);

        var attachment = submission.Documents.Single();
        attachment.Id.Value.Should().NotBe(Guid.Empty);
        attachment.ProductDocumentId.Should().Be(doc);
        attachment.DocumentVersionId.Should().Be(version);
        attachment.AttachedAt.Should().NotBe(default);
    }

    [Fact]
    public void Attach_WithEmptyProductDocument_Throws()
    {
        var submission = NewDraft();

        var act = () => submission.AttachDocument(
            default, DocumentVersionId.New());

        act.Should().Throw<DomainException>()
            .WithMessage($"{SubmissionErrors.ProductDocumentRequired}*");
    }

    [Fact]
    public void Attach_WithEmptyVersion_Throws()
    {
        var submission = NewDraft();

        var act = () => submission.AttachDocument(
            ProductDocumentId.New(), default);

        act.Should().Throw<DomainException>()
            .WithMessage($"{SubmissionErrors.DocumentVersionRequired}*");
    }

    // --- Duplicate protection ------------------------------------------------

    [Fact]
    public void Attach_SameProductDocumentTwice_Throws()
    {
        var submission = NewDraft();
        var (doc, version) = NewRef();
        submission.AttachDocument(doc, version);

        // Even a different version of the same document is rejected — one
        // entry per Product Document.
        var act = () => submission.AttachDocument(doc, DocumentVersionId.New());

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.ProductDocumentAlreadyAttached);
        submission.Documents.Should().ContainSingle();
    }

    // --- Remove --------------------------------------------------------------

    [Fact]
    public void Remove_AttachedDocument_RemovesIt()
    {
        var submission = NewDraft();
        var (doc, version) = NewRef();
        submission.AttachDocument(doc, version);
        var id = submission.Documents.Single().Id;

        submission.RemoveDocument(id);

        submission.Documents.Should().BeEmpty();
    }

    [Fact]
    public void Remove_UnknownDocument_Throws()
    {
        var submission = NewDraft();

        var act = () => submission.RemoveDocument(SubmissionDocumentId.New());

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.DocumentNotAttached);
    }

    // --- Draft protection ----------------------------------------------------

    [Fact]
    public void Attach_WhenPublished_Throws()
    {
        var submission = NewDraft();
        submission.Publish(0, null, [], DateTimeOffset.UtcNow);

        var (doc, version) = NewRef();
        var act = () => submission.AttachDocument(doc, version);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.DocumentsLockedUnlessDraft);
    }

    [Fact]
    public void Remove_WhenPublished_Throws()
    {
        var submission = NewDraft();
        var (doc, version) = NewRef();
        submission.AttachDocument(doc, version);
        var id = submission.Documents.Single().Id;
        submission.Publish(0, null, [], DateTimeOffset.UtcNow);

        var act = () => submission.RemoveDocument(id);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.DocumentsLockedUnlessDraft);
    }

    // --- Collection integrity ------------------------------------------------

    [Fact]
    public void DisplayOrders_AreUnique()
    {
        var submission = NewDraft();
        for (var i = 0; i < 4; i++)
        {
            var (doc, version) = NewRef();
            submission.AttachDocument(doc, version);
        }

        var orders = submission.Documents.Select(d => d.DisplayOrder).ToList();

        orders.Should().OnlyHaveUniqueItems();
        orders.Should().BeEquivalentTo(new[] { 1, 2, 3, 4 });
    }

    [Fact]
    public void Remove_DoesNotCorruptRemainingAttachments()
    {
        var submission = NewDraft();
        var a = NewRef();
        var b = NewRef();
        var c = NewRef();
        submission.AttachDocument(a.Doc, a.Version); // order 1
        submission.AttachDocument(b.Doc, b.Version); // order 2
        submission.AttachDocument(c.Doc, c.Version); // order 3

        var middle = submission.Documents.Single(d => d.ProductDocumentId == b.Doc);
        submission.RemoveDocument(middle.Id);

        // Survivors keep their identities and original order — no renumbering.
        submission.Documents.Should().HaveCount(2);
        submission.Documents.Single(d => d.ProductDocumentId == a.Doc)
            .DisplayOrder.Should().Be(1);
        submission.Documents.Single(d => d.ProductDocumentId == c.Doc)
            .DisplayOrder.Should().Be(3);
    }

    [Fact]
    public void Attach_AfterRemovingLast_ContinuesFromHighestOrder()
    {
        var submission = NewDraft();
        var a = NewRef();
        var b = NewRef();
        submission.AttachDocument(a.Doc, a.Version); // order 1
        submission.AttachDocument(b.Doc, b.Version); // order 2

        var second = submission.Documents.Single(d => d.ProductDocumentId == b.Doc);
        submission.RemoveDocument(second.Id); // highest remaining is now 1

        var c = NewRef();
        submission.AttachDocument(c.Doc, c.Version);

        // max(existing) + 1 = 2 — a gap-free continuation here, but the rule is
        // "highest + 1", not "count + 1".
        submission.Documents.Single(d => d.ProductDocumentId == c.Doc)
            .DisplayOrder.Should().Be(2);
    }

    // --- Lifecycle enabler ---------------------------------------------------

    [Fact]
    public void Publish_FromDraft_BecomesPublished()
    {
        var submission = NewDraft();

        submission.Publish(0, null, [], DateTimeOffset.UtcNow);

        submission.Status.Should().Be(SubmissionStatus.Published);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_Throws()
    {
        var submission = NewDraft();
        submission.Publish(0, null, [], DateTimeOffset.UtcNow);

        var act = () => submission.Publish(0, null, [], DateTimeOffset.UtcNow);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.SubmissionNotDraft);
    }
}
