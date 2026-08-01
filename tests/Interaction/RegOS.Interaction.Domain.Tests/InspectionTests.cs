using FluentAssertions;

using RegOS.Interaction.Domain.Inspections;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Interaction.Domain.Tests;

public sealed class InspectionTests
{
    private static readonly DateOnly Raised = new(2026, 1, 15);

    private static Inspection Begin(
        InspectionStatus initial = InspectionStatus.Announced,
        OrganizationSiteId? site = null)
        => Inspection.Begin(
            TenantId.New(),
            new AuthorityId(Guid.NewGuid()),
            "Pre-approval inspection",
            initial,
            Raised,
            site);

    [Fact]
    public void AnAnnouncedInspectionAndASurpriseOneHaveDifferentBeginnings()
    {
        Begin().CurrentStatus.Should().Be(InspectionStatus.Announced);

        var surprise = Begin(InspectionStatus.InProgress);

        // They arrived unannounced. Forcing it through Announced would put a
        // notice in the history that was never given.
        surprise.History.Should().ContainSingle();
        surprise.History[0].Status.Should().Be(InspectionStatus.InProgress);
    }

    [Fact]
    public void TheSiteMayBeUnknownWhenTheNoticeArrives()
    {
        var inspection = Begin();

        // "The FDA will inspect us in March" comes before "at Plant A".
        inspection.OrganizationSiteId.Should().BeNull();

        var site = OrganizationSiteId.From(Guid.NewGuid());
        inspection.InspectedAt(site);

        inspection.OrganizationSiteId.Should().Be(site);
    }

    [Fact]
    public void ThereIsNoObservationEntityBecauseAnObservationIsNotAQuestion()
    {
        var inspection = Begin();

        // A question asks for information and answering it IS the work. An
        // observation asserts a deficiency and responding to it CREATES work —
        // a Commitment, which already exists. An observation entity would only
        // produce commitments.
        inspection.GetType()
            .GetProperties()
            .Select(x => x.Name)
            .Should()
            .NotContain(name =>
                name.Contains("Observation", StringComparison.Ordinal)
                || name.Contains("Question", StringComparison.Ordinal)
                || name.Contains("Commitment", StringComparison.Ordinal));
    }

    [Fact]
    public void FindingsCannotBeRecordedBeforeItFinishes()
    {
        var inspection = Begin();

        var act = () => inspection.RecordFindings("Three observations issued.");

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(InspectionErrors.OutcomeBeforeCompleted);
    }

    [Fact]
    public void ACompletedInspectionRecordsWhatTheAuthorityFound()
    {
        var inspection = Begin();
        inspection.ChangeStatus(InspectionStatus.InProgress, new DateOnly(2026, 3, 2));
        inspection.ChangeStatus(InspectionStatus.Completed, new DateOnly(2026, 3, 6));

        inspection.RecordFindings("Form 483 issued with three observations.");

        inspection.CompletedOn.Should().Be(new DateOnly(2026, 3, 6));
        inspection.Outcome.Should().Be("Form 483 issued with three observations.");
    }

    [Fact]
    public void TheMiddleStateMayBeSkippedBecausePeopleDoNotAlwaysLogIt()
    {
        var inspection = Begin();

        // No transition table, unlike a meeting: the progression is a natural
        // sequence rather than a fork the authority chooses.
        inspection.ChangeStatus(InspectionStatus.Completed, new DateOnly(2026, 3, 6));

        inspection.CurrentStatus.Should().Be(InspectionStatus.Completed);
    }

    [Fact]
    public void AConcludedInspectionIsTerminal()
    {
        var inspection = Begin();
        inspection.ChangeStatus(InspectionStatus.Cancelled, new DateOnly(2026, 2, 1));

        var act = () => inspection.ChangeStatus(
            InspectionStatus.InProgress, new DateOnly(2026, 3, 1));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(InspectionErrors.AlreadyConcluded);
    }

    [Fact]
    public void AnInspectionCannotBecomeAnnouncedAgain()
    {
        var inspection = Begin(InspectionStatus.InProgress);

        var act = () => inspection.ChangeStatus(
            InspectionStatus.Announced, new DateOnly(2026, 3, 1));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(InspectionErrors.CannotReturnToAnnounced);
    }

    [Theory]
    [InlineData(InspectionStatus.Completed)]
    [InlineData(InspectionStatus.Cancelled)]
    public void AnInspectionCannotBeginInAStateThatHappensToIt(InspectionStatus initial)
    {
        var act = () => Begin(initial);

        act.Should().Throw<DomainException>()
            .WithMessage(InspectionErrors.InvalidInitialStatus);
    }

    [Fact]
    public void TheHistoryCannotGoBackwards()
    {
        var inspection = Begin();

        var act = () => inspection.ChangeStatus(
            InspectionStatus.InProgress, new DateOnly(2025, 1, 1));

        act.Should().Throw<DomainException>()
            .WithMessage(InspectionErrors.HistoryOutOfOrder);
    }
}
