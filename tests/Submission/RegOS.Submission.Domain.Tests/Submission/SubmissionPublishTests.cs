using FluentAssertions;

using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Domain.Tests.Submission;

public class SubmissionPublishTests
{
    private static SubmissionAggregate NewDraft() =>
        SubmissionAggregate.Create(
            new RegulatoryApplicationId(Guid.NewGuid()),
            new SubmissionTypeId(Guid.NewGuid()),
            "Initial 510(k)");

    [Fact]
    public void Publish_FromDraft_TransitionsToPublished()
    {
        var submission = NewDraft();

        submission.Publish();

        submission.Status.Should().Be(SubmissionStatus.Published);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_Throws()
    {
        var submission = NewDraft();
        submission.Publish();

        var act = () => submission.Publish();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage(SubmissionErrors.SubmissionNotDraft);
    }
}
