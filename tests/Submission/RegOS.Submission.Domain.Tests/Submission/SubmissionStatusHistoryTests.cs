using FluentAssertions;

using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Primitives;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Domain.Tests.Submission;

/// <summary>
/// A submission's own lifecycle, and only its own (ADR-046).
/// </summary>
public class SubmissionStatusHistoryTests
{
    private static readonly DateTimeOffset PublishedAt =
        new(2026, 8, 2, 9, 30, 0, TimeSpan.Zero);

    private static SubmissionAggregate NewDraft() =>
        SubmissionAggregate.Create(
            TenantId.New(),
            new RegulatoryApplicationId(Guid.NewGuid()),
            new SubmissionTypeId(Guid.NewGuid()),
            "Original IND");

    /// <summary>
    /// The history begins at creation, not at publication — otherwise a
    /// submission's record would start midway through its own life.
    /// </summary>
    [Fact]
    public void ADraft_AlreadyHasAHistory()
    {
        var submission = NewDraft();

        submission.History.Should().ContainSingle();
        submission.History.Single().Status.Should().Be(SubmissionStatus.Draft);
    }

    [Fact]
    public void Publishing_AppendsRatherThanReplaces()
    {
        var submission = NewDraft();

        submission.Publish(0, null, [], PublishedAt);

        submission.History.Select(x => x.Status).Should().Equal(
            SubmissionStatus.Draft, SubmissionStatus.Published);
    }

    [Fact]
    public void TheEntry_CarriesBothClocks()
    {
        var submission = NewDraft();

        submission.Publish(0, null, [], PublishedAt);

        var entry = submission.History.Last();
        // When it happened, as a regulator would date it...
        entry.OccurredOn.Should().Be(new DateOnly(2026, 8, 2));
        // ...and when RegOS was told.
        entry.RecordedOnUtc.Should().Be(PublishedAt.UtcDateTime);
    }

    /// <summary>
    /// <c>PublishedAt</c> was a column until the history arrived beside it. It
    /// is the same fact, so it is read from the record rather than kept as a
    /// copy that could disagree — the <c>Commitment.GivenOn</c> call again.
    /// </summary>
    [Fact]
    public void PublishedAt_IsReadFromTheHistory()
    {
        var submission = NewDraft();
        submission.PublishedAt.Should().BeNull();

        submission.Publish(0, null, [], PublishedAt);

        submission.PublishedAt.Should().Be(
            submission.History.Last().RecordedOnUtc);
    }

    /// <summary>
    /// <c>Filed</c> is defined and unreachable. Until EPIC-007 transmits, the
    /// package that reaches the authority is assembled outside RegOS, so
    /// nothing here can honestly claim a submission was filed.
    /// </summary>
    [Fact]
    public void NothingTransitionsToFiled()
    {
        var submission = NewDraft();

        submission.Publish(0, null, [], PublishedAt);

        submission.Status.Should().Be(SubmissionStatus.Published);
        submission.History.Should().NotContain(
            x => x.Status == SubmissionStatus.Filed);
    }

    /// <summary>
    /// The authority's words are absent from this enum by design, and this test
    /// exists so adding one is a deliberate act rather than a quiet extension.
    /// </summary>
    [Fact]
    public void TheEnumHoldsOnlyStatesWeAreTheActorOf()
    {
        Enum.GetNames<SubmissionStatus>().Should().Equal(
            nameof(SubmissionStatus.Draft),
            nameof(SubmissionStatus.Published),
            nameof(SubmissionStatus.Filed));
    }
}
