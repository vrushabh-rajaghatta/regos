using FluentAssertions;

using RegOS.ReferenceData.Domain.SubmissionType;
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
        SubmissionAggregate.Create(
            new RegulatoryApplicationId(Guid.NewGuid()),
            new SubmissionTypeId(Guid.NewGuid()),
            "Initial 510(k)");

    [Fact]
    public void Draft_HasNoPublicationMetadata()
    {
        var submission = NewDraft();

        submission.Status.Should().Be(SubmissionStatus.Draft);
        submission.PublishedAt.Should().BeNull();
    }

    [Fact]
    public void Publish_FromDraft_SetsStatusAndPublishedAt()
    {
        var submission = NewDraft();

        submission.Publish(PublishedAt);

        submission.Status.Should().Be(SubmissionStatus.Published);
        submission.PublishedAt.Should().Be(PublishedAt);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_ThrowsAndKeepsOriginalPublishedAt()
    {
        var submission = NewDraft();
        submission.Publish(PublishedAt);

        var act = () => submission.Publish(PublishedAt.AddDays(1));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.SubmissionNotDraft);
        // The original publication timestamp is unchanged.
        submission.PublishedAt.Should().Be(PublishedAt);
    }

    [Fact]
    public void Publish_WithDefaultTimestamp_Throws()
    {
        var submission = NewDraft();

        var act = () => submission.Publish(default);

        act.Should().Throw<DomainException>()
            .WithMessage(SubmissionErrors.PublishedAtRequired + "*");
        // Rejected before any state change.
        submission.Status.Should().Be(SubmissionStatus.Draft);
        submission.PublishedAt.Should().BeNull();
    }
}
