using FluentAssertions;

using RegOS.Interaction.Domain.Commitments;
using RegOS.Platform.Contracts;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Interaction.Domain.Tests;

public sealed class CommitmentTests
{
    private static readonly DateOnly Given = new(2024, 6, 1);
    private static readonly DateOnly Due = new(2026, 6, 1);

    private static Commitment Give(UserId? owner = null)
        => Commitment.Give(
            TenantId.New(),
            new AuthorityId(Guid.NewGuid()),
            "Submit five-year stability data",
            Given,
            Due,
            ownerUserId: owner);

    [Fact]
    public void TheDateItWasGivenIsDerivedFromTheHistoryRatherThanStored()
    {
        var commitment = Give();

        commitment.GivenOn.Should().Be(Given);
        commitment.History.Should().ContainSingle();
        commitment.History[0].OccurredOn.Should().Be(Given);

        // A stored copy could disagree with the history beside it. Same call
        // LaunchedOn and RespondedOn made.
        commitment.GetType()
            .GetProperties()
            .Single(p => p.Name == nameof(Commitment.GivenOn))
            .CanWrite.Should().BeFalse();
    }

    [Fact]
    public void ThereIsNoWayToRecordThatWeFailed()
    {
        // The decisive absence. A commitment we did not do is Open and past
        // its date; whether that matters is the authority's judgement, recorded
        // in a letter, not ours to store.
        Enum.GetNames<CommitmentStatus>()
            .Should()
            .BeEquivalentTo("Open", "InProgress", "Fulfilled", "Waived");
    }

    [Fact]
    public void OverdueIsNotAStatusItIsAReadingOfTwoFacts()
    {
        var commitment = Give();

        // Everything a caller needs to say "overdue" is here; nothing says it
        // for them, because it changes every midnight.
        commitment.DueOn.Should().Be(Due);
        commitment.CurrentStatus.Should().Be(CommitmentStatus.Open);
        commitment.FulfilledOn.Should().BeNull();
    }

    [Fact]
    public void FulfillingIsOursAndWaivingIsTheirs()
    {
        var fulfilled = Give();
        fulfilled.ChangeStatus(CommitmentStatus.Fulfilled, new DateOnly(2026, 5, 1));
        fulfilled.FulfilledOn.Should().Be(new DateOnly(2026, 5, 1));

        var waived = Give();
        waived.ChangeStatus(CommitmentStatus.Waived, new DateOnly(2026, 5, 1));

        // Waived is not a kind of fulfilment: nothing was performed, the
        // obligation was released.
        waived.CurrentStatus.Should().Be(CommitmentStatus.Waived);
        waived.FulfilledOn.Should().BeNull();
    }

    [Fact]
    public void AClosedCommitmentIsTerminalInBothDirections()
    {
        var commitment = Give();
        commitment.ChangeStatus(CommitmentStatus.Fulfilled, new DateOnly(2026, 5, 1));

        var again = () => commitment.ChangeStatus(
            CommitmentStatus.InProgress, new DateOnly(2026, 5, 2));

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(CommitmentErrors.AlreadyClosed);
    }

    [Fact]
    public void ACommitmentCannotReturnToOpen()
    {
        var commitment = Give();
        commitment.ChangeStatus(CommitmentStatus.InProgress, new DateOnly(2025, 1, 1));

        var reopen = () => commitment.ChangeStatus(
            CommitmentStatus.Open, new DateOnly(2025, 2, 1));

        // Open means "promised, not started". A commitment already started
        // cannot become unstarted — the word's meaning enforces the rule.
        reopen.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(CommitmentErrors.CannotReopen);
    }

    [Fact]
    public void TheHistoryCannotGoBackwards()
    {
        var commitment = Give();

        var act = () => commitment.ChangeStatus(
            CommitmentStatus.InProgress, new DateOnly(2024, 1, 1));

        act.Should().Throw<DomainException>()
            .WithMessage(CommitmentErrors.HistoryOutOfOrder);
    }

    [Fact]
    public void AnOwnerCanBeAssignedAndCleared()
    {
        var owner = UserId.New();
        var commitment = Give(owner);

        commitment.OwnerUserId.Should().Be(owner);

        // Unassigning is a real act — a queue with nobody on it is a state the
        // business has, not an error.
        commitment.AssignTo(null);
        commitment.OwnerUserId.Should().BeNull();
    }

    [Fact]
    public void ACommitmentIsMadeToAnAuthorityAndCannotExistWithoutOne()
    {
        var act = () => Commitment.Give(
            TenantId.New(), default, "x", Given, Due);

        act.Should().Throw<DomainException>()
            .WithMessage(CommitmentErrors.AuthorityRequired);
    }
}
