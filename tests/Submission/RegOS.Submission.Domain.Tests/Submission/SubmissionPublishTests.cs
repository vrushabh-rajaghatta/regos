using RegOS.SharedKernel.Primitives;
using FluentAssertions;

using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Submission.Domain.Tests.Submission;

public class SubmissionPublishTests
{
    private static readonly DateTimeOffset PublishedAt =
        new(2026, 7, 19, 10, 15, 0, TimeSpan.Zero);

    private static SubmissionAggregate NewDraft() =>
        SubmissionAggregate.Create(TenantId.New(), 
            new RegulatoryApplicationId(Guid.NewGuid()),
            "Initial 510(k)",
            SubmissionFormat.Ectd);

    [Fact]
    public void Draft_HasNoPublicationMetadata()
    {
        var submission = NewDraft();

        submission.Status.Should().Be(SubmissionStatus.Draft);
        submission.PublishedAt.Should().BeNull();
        // A draft has not been transmitted, so it has no number to carry.
        submission.SequenceNumber.Should().BeNull();
    }

    [Fact]
    public void Publish_FromDraft_SetsStatusAndPublishedAt()
    {
        var submission = NewDraft();

        submission.Publish(0, null, [], PublishedAt);

        submission.Status.Should().Be(SubmissionStatus.Published);
        submission.PublishedAt.Should().Be(PublishedAt.UtcDateTime);
    }

    // --- Sequence numbering (ADR-044) ----------------------------------------

    /// <summary>
    /// The empty-baseline case, asserted rather than assumed: eCTD numbering
    /// starts at 0000, so the first sequence in an application has no
    /// predecessor and must be exactly zero.
    /// </summary>
    [Fact]
    public void Publish_TheFirstSequenceInAnApplication_Is0000()
    {
        var submission = NewDraft();

        submission.Publish(0, previousPublishedSequenceNumber: null, [], PublishedAt);

        submission.SequenceNumber.Should().Be(0);
    }

    [Fact]
    public void Publish_TheFirstSequence_MustBeZeroAndNotOne()
    {
        var submission = NewDraft();

        var act = () => submission.Publish(1, null, [], PublishedAt);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.SequenceNumberNotContiguous);
        submission.Status.Should().Be(SubmissionStatus.Draft);
        submission.SequenceNumber.Should().BeNull();
    }

    [Fact]
    public void Publish_FollowingAnExistingSequence_TakesTheNextNumber()
    {
        var submission = NewDraft();

        submission.Publish(3, previousPublishedSequenceNumber: 2, [], PublishedAt);

        submission.SequenceNumber.Should().Be(3);
    }

    /// <summary>
    /// A gap is what the aggregate exists to refuse. It cannot see whether 0006
    /// really exists — a Submission is a root and its siblings are outside its
    /// consistency boundary — but it can refuse a number that does not follow
    /// the one it was told about (ADR-044 decision 6).
    /// </summary>
    [Theory]
    [InlineData(7, 5)]   // skips 0006
    [InlineData(2, 5)]   // goes backwards
    [InlineData(5, 5)]   // repeats the previous
    public void Publish_WithANumberThatDoesNotFollowTheLast_IsRefused(
        int sequenceNumber, int previous)
    {
        var submission = NewDraft();

        var act = () => submission.Publish(sequenceNumber, previous, [], PublishedAt);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.SequenceNumberNotContiguous);
        // Refused before any state change — still a draft, still unnumbered.
        submission.Status.Should().Be(SubmissionStatus.Draft);
        submission.SequenceNumber.Should().BeNull();
    }

    [Fact]
    public void Publish_WithANegativeSequenceNumber_Throws()
    {
        var submission = NewDraft();

        var act = () => submission.Publish(-1, null, [], PublishedAt);

        act.Should().Throw<DomainException>()
            .WithMessage(SubmissionErrors.SequenceNumberNotNegative + "*");
        submission.SequenceNumber.Should().BeNull();
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_ThrowsAndKeepsOriginalPublishedAt()
    {
        var submission = NewDraft();
        submission.Publish(0, null, [], PublishedAt);

        var act = () => submission.Publish(1, 0, [], PublishedAt.AddDays(1));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.SubmissionNotDraft);
        // The original publication timestamp is unchanged.
        submission.PublishedAt.Should().Be(PublishedAt.UtcDateTime);
    }

    [Fact]
    public void Publish_WithDefaultTimestamp_Throws()
    {
        var submission = NewDraft();

        var act = () => submission.Publish(0, null, [], default);

        act.Should().Throw<DomainException>()
            .WithMessage(SubmissionErrors.PublishedAtRequired + "*");
        // Rejected before any state change.
        submission.Status.Should().Be(SubmissionStatus.Draft);
        submission.PublishedAt.Should().BeNull();
    }
}
