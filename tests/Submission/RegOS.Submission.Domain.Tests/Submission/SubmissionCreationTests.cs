using RegOS.SharedKernel.Primitives;
using FluentAssertions;

using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Domain.Tests.Submission;

public class SubmissionCreationTests
{
    private static SubmissionAggregate NewDraft() =>
        SubmissionAggregate.Create(TenantId.New(), 
            new RegulatoryApplicationId(Guid.NewGuid()),
            "Initial 510(k)",
            SubmissionFormat.Ectd);

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

    [Fact]
    public void Create_WithoutATemplate_LeavesTheSubmissionUnbound()
    {
        // Reference data that has no published blueprint (device submissions
        // today) must never block creating a submission.
        NewDraft().BoundTemplateVersionId.Should().BeNull();
    }

    [Fact]
    public void Create_PinsTheBoundTemplateVersion()
    {
        var versionId = RegulatoryTemplateVersionId.New();

        var submission = SubmissionAggregate.Create(
            TenantId.New(),
            new RegulatoryApplicationId(Guid.NewGuid()),
            "Initial IND",
            SubmissionFormat.Ectd,
            versionId);

        submission.BoundTemplateVersionId.Should().Be(versionId);
    }
}
