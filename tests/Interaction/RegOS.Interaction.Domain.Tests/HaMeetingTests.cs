using FluentAssertions;

using RegOS.Interaction.Domain.Meetings;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Interaction.Domain.Tests;

public sealed class HaMeetingTests
{
    private static readonly DateOnly Raised = new(2026, 2, 1);

    private static HaMeeting Begin(
        HaMeetingStatus initial = HaMeetingStatus.Requested)
        => HaMeeting.Begin(
            TenantId.New(),
            new AuthorityId(Guid.NewGuid()),
            "Type B meeting on the Phase 3 design",
            initial,
            Raised);

    [Fact]
    public void AMeetingWeAskedForAndOneTheyCalledHaveDifferentBeginnings()
    {
        Begin(HaMeetingStatus.Requested).CurrentStatus
            .Should().Be(HaMeetingStatus.Requested);

        var summoned = Begin(HaMeetingStatus.Granted);

        // Not "requested then immediately granted": that would put a request in
        // the history that never happened. Provenance, not convenience.
        summoned.CurrentStatus.Should().Be(HaMeetingStatus.Granted);
        summoned.History.Should().ContainSingle();
        summoned.History[0].Status.Should().Be(HaMeetingStatus.Granted);
    }

    [Theory]
    [InlineData(HaMeetingStatus.Held)]
    [InlineData(HaMeetingStatus.Declined)]
    [InlineData(HaMeetingStatus.Cancelled)]
    public void AMeetingCannotBeginInAStateThatHappensToAMeeting(HaMeetingStatus initial)
    {
        var act = () => Begin(initial);

        act.Should().Throw<DomainException>()
            .WithMessage(HaMeetingErrors.InvalidInitialStatus);
    }

    [Fact]
    public void TheAuthorityChoosesTheBranchAndTheTableSaysSo()
    {
        var granted = Begin();
        granted.ChangeStatus(HaMeetingStatus.Granted, new DateOnly(2026, 2, 10));
        granted.CurrentStatus.Should().Be(HaMeetingStatus.Granted);

        var declined = Begin();
        declined.ChangeStatus(HaMeetingStatus.Declined, new DateOnly(2026, 2, 10));
        declined.CurrentStatus.Should().Be(HaMeetingStatus.Declined);
    }

    [Fact]
    public void AMeetingCannotBeHeldBeforeItIsGranted()
    {
        var meeting = Begin();

        var act = () => meeting.ChangeStatus(
            HaMeetingStatus.Held, new DateOnly(2026, 3, 1));

        // The one transition table in the context, and this is what it is for.
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(HaMeetingErrors.TransitionNotAllowed);
    }

    [Theory]
    [InlineData(HaMeetingStatus.Declined)]
    [InlineData(HaMeetingStatus.Cancelled)]
    public void EveryConclusionIsTerminalBecauseASecondMeetingIsASecondMeeting(
        HaMeetingStatus conclusion)
    {
        var meeting = Begin();
        meeting.ChangeStatus(conclusion, new DateOnly(2026, 2, 10));

        var act = () => meeting.ChangeStatus(
            HaMeetingStatus.Granted, new DateOnly(2026, 2, 11));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(HaMeetingErrors.AlreadyConcluded);
    }

    [Fact]
    public void AnOutcomeCannotBeRecordedForAMeetingThatHasNotHappened()
    {
        var meeting = Begin();
        meeting.ChangeStatus(HaMeetingStatus.Granted, new DateOnly(2026, 2, 10));

        var act = () => meeting.RecordOutcome("Notes", "Agreed");

        // Minutes of a meeting that has not taken place are a plan, and this
        // aggregate does not hold plans.
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(HaMeetingErrors.OutcomeBeforeHeld);
    }

    [Fact]
    public void AHeldMeetingRecordsWhatWasSaidAndWhatTheAuthorityConcluded()
    {
        var meeting = Begin();
        meeting.ChangeStatus(HaMeetingStatus.Granted, new DateOnly(2026, 2, 10));
        meeting.ChangeStatus(HaMeetingStatus.Held, new DateOnly(2026, 3, 5));

        meeting.RecordOutcome(
            "Discussed the proposed Phase 3 design.",
            "The agency accepted the proposed Phase 3 design.");

        meeting.HeldOn.Should().Be(new DateOnly(2026, 3, 5));
        meeting.Outcome.Should().Be("The agency accepted the proposed Phase 3 design.");

        // The outcome is their position. What we now owe lives on Commitment,
        // with its own due date, owner and history.
        meeting.GetType()
            .GetProperties()
            .Select(x => x.Name)
            .Should()
            .NotContain(name => name.Contains("Commitment", StringComparison.Ordinal));
    }

    [Fact]
    public void TheHistoryCannotGoBackwards()
    {
        var meeting = Begin();

        var act = () => meeting.ChangeStatus(
            HaMeetingStatus.Granted, new DateOnly(2026, 1, 1));

        act.Should().Throw<DomainException>()
            .WithMessage(HaMeetingErrors.HistoryOutOfOrder);
    }

    [Fact]
    public void TheDateItWasRaisedIsDerivedFromTheHistory()
    {
        var meeting = Begin();

        meeting.RaisedOn.Should().Be(Raised);
        meeting.HeldOn.Should().BeNull();
    }
}
