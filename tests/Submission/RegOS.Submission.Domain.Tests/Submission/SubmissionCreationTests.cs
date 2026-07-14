using FluentAssertions;

using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Domain.Tests.Submission;

public class SubmissionCreationTests
{
    private static SubmissionAggregate NewDraft() =>
        SubmissionAggregate.Create(
            new RegulatoryApplicationId(Guid.NewGuid()),
            new SubmissionTypeId(Guid.NewGuid()),
            "Initial 510(k)");

    [Fact]
    public void Create_StartsInDraft()
    {
        NewDraft().Status.Should().Be(SubmissionStatus.Draft);
    }

    [Fact]
    public void Create_PopulatesCreatedOn()
    {
        NewDraft().CreatedOn.Should().NotBe(default);
    }
}
