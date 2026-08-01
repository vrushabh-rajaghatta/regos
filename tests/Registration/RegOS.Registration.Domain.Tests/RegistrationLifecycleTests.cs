using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

using RegistrationAggregate = RegOS.Registration.Domain.Aggregates.Registration.Registration;

namespace RegOS.Registration.Domain.Tests;

/// <summary>
/// The lifecycle: which statuses may follow which, and the invariant that holds
/// however a registration got where it is.
/// </summary>
public class RegistrationLifecycleTests
{
    private static readonly DateOnly Start = new(2020, 1, 1);

    private static readonly RegistrationStatus[] AllStatuses =
        Enum.GetValues<RegistrationStatus>();

    // --- The invariant -------------------------------------------------------

    /// <summary>
    /// The core invariant, asserted for <em>every</em> legal transition rather
    /// than the interesting few: each one updates the current status and appends
    /// exactly one immutable history entry. If this holds universally, current
    /// state and the record of how it was reached can never disagree.
    /// </summary>
    [Theory]
    [MemberData(nameof(EveryPermittedTransition))]
    public void EveryTransitionChangesStatusAndAppendsExactlyOneEntry(
        RegistrationStatus from,
        RegistrationStatus to)
    {
        var registration = Reach(from);
        var before = registration.History.Count;
        var effective = Start.AddYears(20);

        Apply(registration, to, effective);

        registration.CurrentStatus.Should().Be(to);
        registration.History.Should().HaveCount(before + 1);

        var appended = registration.History.Last();
        appended.Status.Should().Be(to);
        appended.OccurredOn.Should().Be(effective);
    }

    [Theory]
    [MemberData(nameof(EveryForbiddenTransition))]
    public void EveryForbiddenTransitionIsRefusedAndChangesNothing(
        RegistrationStatus from,
        RegistrationStatus to)
    {
        var registration = Reach(from);
        var before = registration.History.Count;

        var change = () => Apply(registration, to, Start.AddYears(20));

        change.Should().Throw<BusinessRuleViolationException>();

        registration.CurrentStatus.Should().Be(from);
        registration.History.Should().HaveCount(before);
    }

    // --- The table -----------------------------------------------------------

    /// <summary>
    /// Suspension is a reversible operational state, not the destruction of the
    /// authorisation: the grant still exists, it merely cannot be exercised.
    /// </summary>
    [Fact]
    public void ASuspendedRegistrationCanBeReinstated()
    {
        var registration = Reach(RegistrationStatus.Suspended);

        registration.ChangeStatus(
            RegistrationStatus.Approved,
            Start.AddYears(5),
            "Suspension lifted.");

        registration.CurrentStatus.Should().Be(RegistrationStatus.Approved);

        // The grant it was given in the first place is untouched.
        registration.RegistrationNumber.Should().NotBeNull();
        registration.ApprovedOn.Should().NotBeNull();
    }

    /// <summary>
    /// A migrated authorisation never passed through RegOS's Submitted or
    /// UnderReview. Recording it as granted is not skipping steps — it is
    /// recording that RegOS entered the story after those steps happened.
    /// </summary>
    [Fact]
    public void APlannedRegistrationCanJumpStraightToGranted()
    {
        var registration = New(occurredOn: new DateOnly(2019, 1, 15));

        registration.RecordApproval("NDA-legacy", new DateOnly(2019, 4, 12));

        registration.CurrentStatus.Should().Be(RegistrationStatus.Approved);
    }

    [Theory]
    [InlineData(RegistrationStatus.Refused)]
    [InlineData(RegistrationStatus.Expired)]
    [InlineData(RegistrationStatus.Withdrawn)]
    public void TerminalStatusesPermitNothing(RegistrationStatus status)
    {
        RegistrationLifecycle.IsTerminal(status).Should().BeTrue();
        RegistrationLifecycle.From(status).Should().BeEmpty();
    }

    [Theory]
    [InlineData(RegistrationStatus.Planned)]
    [InlineData(RegistrationStatus.Submitted)]
    [InlineData(RegistrationStatus.UnderReview)]
    [InlineData(RegistrationStatus.Approved)]
    [InlineData(RegistrationStatus.Suspended)]
    public void LiveStatusesPermitSomething(RegistrationStatus status)
    {
        RegistrationLifecycle.IsTerminal(status).Should().BeFalse();
    }

    /// <summary>
    /// Staying put while something else changes is a different operation — a
    /// renewal keeps the status at Approved and moves the validity dates — so no
    /// status permits itself.
    /// </summary>
    [Fact]
    public void NoStatusPermitsItself()
    {
        foreach (var status in AllStatuses)
        {
            RegistrationLifecycle.Permits(status, status).Should().BeFalse(
                "{0} should not be reachable from itself", status);
        }
    }

    /// <summary>
    /// The table must answer for every status in the enum, so adding a status
    /// without deciding where it may go is a failing test rather than a silent
    /// dead end.
    /// </summary>
    [Fact]
    public void EveryStatusIsReachableFromCreation()
    {
        foreach (var status in AllStatuses)
        {
            var reach = () => Reach(status);

            reach.Should().NotThrow(
                "{0} should be reachable from a new registration", status);
        }
    }

    // --- Refusals ------------------------------------------------------------

    [Fact]
    public void AlreadyBeingInTheTargetStatusSaysSo()
    {
        var registration = Reach(RegistrationStatus.Submitted);

        var change = () => registration.ChangeStatus(
            RegistrationStatus.Submitted, Start.AddYears(1));

        change.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegistrationErrors.AlreadyInStatus(
                RegistrationStatus.Submitted));
    }

    [Fact]
    public void ATerminalRegistrationSaysItsLifecycleHasEnded()
    {
        var registration = Reach(RegistrationStatus.Refused);

        var change = () => registration.ChangeStatus(
            RegistrationStatus.Withdrawn, Start.AddYears(1));

        change.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegistrationErrors.StatusIsTerminal(
                RegistrationStatus.Refused));
    }

    [Fact]
    public void AnIncoherentTransitionNamesBothEnds()
    {
        var registration = New();

        var change = () => registration.ChangeStatus(
            RegistrationStatus.Suspended, Start.AddYears(1));

        change.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegistrationErrors.TransitionNotPermitted(
                RegistrationStatus.Planned, RegistrationStatus.Suspended));
    }

    /// <summary>
    /// The behaviour STORY-002 changes: a refused registration was previously
    /// approvable. Nothing was ever granted, so there is nothing to grant.
    /// </summary>
    [Fact]
    public void ARefusedRegistrationCannotBeApproved()
    {
        var registration = Reach(RegistrationStatus.Refused);

        var approve = () => registration.RecordApproval(
            "NDA-1", Start.AddYears(1));

        approve.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegistrationErrors.StatusIsTerminal(
                RegistrationStatus.Refused));
    }

    /// <summary>
    /// The first grant establishes the registration number and validity dates,
    /// which a plain status change has no way to supply — so it cannot be the
    /// door into Approved.
    /// </summary>
    [Fact]
    public void ChangingStatusCannotPerformTheFirstGrant()
    {
        var registration = New();

        var change = () => registration.ChangeStatus(
            RegistrationStatus.Approved, Start.AddYears(1));

        change.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegistrationErrors.ApprovalMustBeRecordedAsAGrant);

        registration.CurrentStatus.Should().Be(RegistrationStatus.Planned);
        registration.RegistrationNumber.Should().BeNull();
    }

    [Fact]
    public void AGrantCannotBeRecordedTwiceEvenAfterTheStatusMovedOn()
    {
        var registration = Reach(RegistrationStatus.Suspended);

        var again = () => registration.RecordApproval(
            "NDA-2", Start.AddYears(6));

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegistrationErrors.ApprovalAlreadyRecorded);
    }

    [Fact]
    public void AStatusOutsideTheEnumIsRejected()
    {
        var registration = New();

        var change = () => registration.ChangeStatus(
            (RegistrationStatus)99, Start.AddYears(1));

        change.Should().Throw<DomainException>()
            .WithMessage(RegistrationErrors.StatusNotRecognised);
    }

    [Fact]
    public void ADateIsRequiredForATransition()
    {
        var registration = New();

        var change = () => registration.ChangeStatus(
            RegistrationStatus.Submitted, default);

        change.Should().Throw<DomainException>()
            .WithMessage(RegistrationErrors.OccurredOnRequired);
    }

    // --- Chronology ----------------------------------------------------------

    [Fact]
    public void AStatusCannotTakeEffectBeforeTheOneItReplaces()
    {
        var registration = New(occurredOn: new DateOnly(2020, 6, 1));

        var change = () => registration.ChangeStatus(
            RegistrationStatus.Submitted, new DateOnly(2020, 5, 31));

        change.Should().Throw<DomainException>()
            .WithMessage(RegistrationErrors.OccurredOnBeforePreviousEntry);
    }

    /// <summary>
    /// Two events on the same business date is ordinary — a migration routinely
    /// produces it — so the rule is non-decreasing, not strictly increasing.
    /// </summary>
    [Fact]
    public void TwoStatusesMayShareABusinessDate()
    {
        var sameDay = new DateOnly(2020, 6, 1);
        var registration = New(occurredOn: sameDay);

        var change = () => registration.ChangeStatus(
            RegistrationStatus.Submitted, sameDay);

        change.Should().NotThrow();
    }

    [Fact]
    public void ARejectedTransitionLeavesTheRegistrationUntouched()
    {
        var registration = Reach(RegistrationStatus.Approved);
        var before = registration.History.Count;

        var change = () => registration.ChangeStatus(
            RegistrationStatus.Submitted, Start.AddYears(10));
        change.Should().Throw<BusinessRuleViolationException>();

        registration.CurrentStatus.Should().Be(RegistrationStatus.Approved);
        registration.History.Should().HaveCount(before);
    }

    // --- Notes ---------------------------------------------------------------

    [Fact]
    public void ATransitionCarriesItsNoteIntoHistory()
    {
        var registration = Reach(RegistrationStatus.Approved);

        registration.ChangeStatus(
            RegistrationStatus.Suspended,
            Start.AddYears(5),
            "GMP non-compliance at the manufacturing site.");

        registration.History.Last().Note
            .Should().Be("GMP non-compliance at the manufacturing site.");
    }

    /// <summary>
    /// The provenance split holds for transitions too: when it happened in the
    /// world, and when RegOS learned of it.
    /// </summary>
    [Fact]
    public void ATransitionRecordsBothWhenItHappenedAndWhenItWasEntered()
    {
        var registration = Reach(RegistrationStatus.Approved);
        var suspended = new DateOnly(2023, 9, 14);

        registration.ChangeStatus(RegistrationStatus.Suspended, suspended);

        var entry = registration.History.Last();

        entry.OccurredOn.Should().Be(suspended);
        entry.RecordedOnUtc.Should()
            .BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    // --- Fixtures ------------------------------------------------------------

    public static TheoryData<RegistrationStatus, RegistrationStatus>
        EveryPermittedTransition()
    {
        var data = new TheoryData<RegistrationStatus, RegistrationStatus>();

        foreach (var from in AllStatuses)
        {
            foreach (var to in RegistrationLifecycle.From(from))
                data.Add(from, to);
        }

        return data;
    }

    public static TheoryData<RegistrationStatus, RegistrationStatus>
        EveryForbiddenTransition()
    {
        var data = new TheoryData<RegistrationStatus, RegistrationStatus>();

        foreach (var from in AllStatuses)
        {
            foreach (var to in AllStatuses)
            {
                if (!RegistrationLifecycle.Permits(from, to))
                    data.Add(from, to);
            }
        }

        return data;
    }

    private static RegistrationAggregate New(DateOnly? occurredOn = null) =>
        RegistrationAggregate.Create(
            TenantId.New(),
            MedicinalProductId.New(),
            new AuthorityId(Guid.NewGuid()),
            new OrganizationId(Guid.NewGuid()),
            occurredOn ?? Start);

    /// <summary>
    /// A registration standing in <paramref name="status"/>, walked there one
    /// legal transition at a time from creation. Breadth-first over the same
    /// table under test, which is safe: what the tests then assert — that a
    /// status changed and exactly one entry was appended — does not depend on
    /// the table being right.
    /// </summary>
    private static RegistrationAggregate Reach(RegistrationStatus status)
    {
        var registration = New();
        var effective = Start;

        foreach (var step in PathTo(status))
        {
            effective = effective.AddDays(30);
            Apply(registration, step, effective);
        }

        return registration;
    }

    /// <summary>The shortest sequence of statuses from Planned, exclusive.</summary>
    private static IReadOnlyList<RegistrationStatus> PathTo(
        RegistrationStatus target)
    {
        if (target == RegistrationStatus.Planned)
            return [];

        var queue = new Queue<RegistrationStatus>([RegistrationStatus.Planned]);
        var cameFrom = new Dictionary<RegistrationStatus, RegistrationStatus>();
        var seen = new HashSet<RegistrationStatus> { RegistrationStatus.Planned };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var next in RegistrationLifecycle.From(current))
            {
                if (!seen.Add(next))
                    continue;

                cameFrom[next] = current;

                if (next == target)
                {
                    var path = new List<RegistrationStatus>();

                    for (var at = target;
                         at != RegistrationStatus.Planned;
                         at = cameFrom[at])
                    {
                        path.Add(at);
                    }

                    path.Reverse();
                    return path;
                }

                queue.Enqueue(next);
            }
        }

        throw new InvalidOperationException(
            $"{target} is not reachable from Planned.");
    }

    /// <summary>
    /// Moves a registration to <paramref name="target"/> through whichever
    /// operation owns that entry: the first grant carries a number and dates, so
    /// it goes through RecordApproval; everything else is a plain transition.
    /// </summary>
    private static void Apply(
        RegistrationAggregate registration,
        RegistrationStatus target,
        DateOnly occurredOn)
    {
        if (target == RegistrationStatus.Approved
            && registration.ApprovedOn is null)
        {
            registration.RecordApproval(
                "NDA-000123",
                occurredOn,
                occurredOn.AddYears(5));

            return;
        }

        registration.ChangeStatus(target, occurredOn);
    }
}
