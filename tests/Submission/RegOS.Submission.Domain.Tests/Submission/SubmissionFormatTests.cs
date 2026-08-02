using FluentAssertions;

using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Domain.Tests.Submission;

/// <summary>
/// What a filing will be rendered as, and when that stops being editable
/// (ADR-047).
/// </summary>
public class SubmissionFormatTests
{
    private static readonly DateTimeOffset PublishedAt =
        new(2026, 8, 2, 9, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(SubmissionFormat.Ectd)]
    [InlineData(SubmissionFormat.Nees)]
    [InlineData(SubmissionFormat.Paper)]
    public void Create_KeepsTheFormatItWasGiven(SubmissionFormat format)
    {
        NewDraft(format).Format.Should().Be(format);
    }

    /// <summary>
    /// The domain takes no default. eCTD is the only format an FDA IND accepts
    /// today, which is exactly what would make a default look harmless — and
    /// would let a caller omit a real decision and have the model answer it.
    /// </summary>
    [Fact]
    public void Create_RefusesAFormatThatIsNotOne()
    {
        var act = () => NewDraft((SubmissionFormat)99);

        act.Should().Throw<DomainException>()
            .WithMessage(SubmissionErrors.FormatNotRecognised);
    }

    [Fact]
    public void ChangeFormat_IsAllowedWhileADraft()
    {
        var submission = NewDraft(SubmissionFormat.Ectd);

        submission.ChangeFormat(SubmissionFormat.Paper);

        submission.Format.Should().Be(SubmissionFormat.Paper);
    }

    [Fact]
    public void ChangeFormat_RefusesAFormatThatIsNotOne()
    {
        var submission = NewDraft(SubmissionFormat.Ectd);

        var act = () => submission.ChangeFormat((SubmissionFormat)99);

        act.Should().Throw<DomainException>()
            .WithMessage(SubmissionErrors.FormatNotRecognised);
    }

    /// <summary>
    /// <b>The freeze.</b> A published sequence's format is a fact about a filing
    /// already made, and no later decision reaches back to alter it. The draft
    /// guard is the whole mechanism — deliberately not a second one, because a
    /// rule with two homes grows two behaviours.
    /// </summary>
    [Fact]
    public void ChangeFormat_IsRefusedOncePublished()
    {
        var submission = NewDraft(SubmissionFormat.Ectd);
        submission.Publish(0, null, [], PublishedAt);

        var act = () => submission.ChangeFormat(SubmissionFormat.Paper);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.FormatLockedOncePublished);

        submission.Format.Should().Be(SubmissionFormat.Ectd);
    }

    /// <summary>
    /// The membership test ADR-047 relies on. Adding DTD version or gateway
    /// format as a value here would be a deliberate act, not a quiet extension
    /// — and both are facts that do not exist until EPIC-007 builds a package.
    /// </summary>
    [Fact]
    public void TheFormatsAreExactlyThese()
    {
        Enum.GetValues<SubmissionFormat>().Should().BeEquivalentTo(
        [
            SubmissionFormat.Ectd,
            SubmissionFormat.Nees,
            SubmissionFormat.Paper
        ]);
    }

    private static SubmissionAggregate NewDraft(SubmissionFormat format) =>
        SubmissionAggregate.Create(
            TenantId.New(),
            new RegulatoryApplicationId(Guid.NewGuid()),
            "Original IND",
            format);
}
