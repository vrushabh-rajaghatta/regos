using RegOS.Platform.Contracts;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Interaction.Domain.Meetings;

/// <summary>
/// A scheduled interaction with a health authority — requested, granted or
/// declined, held, and concluded.
/// </summary>
/// <remarks>
/// <b>The only object in this context that concludes.</b> Correspondence,
/// questions and commitments generate ongoing work; a meeting's value lies in
/// the work it <em>produces</em> — commitments, follow-up questions, a recorded
/// position — not in a continuing lifecycle of its own. Designing it by asking
/// <em>"what work remains after the meeting?"</em> rather than <em>"what
/// happened at the meeting?"</em> is why this aggregate is small.
/// <para>
/// <b>The initial status is a parameter, and that preserves provenance.</b> We
/// request some meetings; an authority calls others. Hard-coding
/// <c>Requested</c> and immediately transitioning the second kind to
/// <c>Granted</c> would put a request in the history that never happened. Two
/// different business events deserve two different beginnings.
/// </para>
/// <para>
/// <b><see cref="Outcome"/> is the authority's position, not a summary of what
/// we now owe.</b> <em>"The agency accepted the proposed Phase 3 design"</em> is
/// an outcome; <em>"submit stability data by Q3"</em> is a
/// <c>Commitment</c> with its own due date, owner and history. Their conclusion
/// and our obligation have different owners — the same test that separated
/// <c>Fulfilled</c> from <c>Waived</c>. If this field ever starts listing
/// actions, the commitments it duplicates already exist elsewhere.
/// </para>
/// <para>
/// <b>No attendees, and not because they are rarely asked for.</b> They answer a
/// different question: <em>"who is coming?"</em> is planning and <em>"who
/// came?"</em> is audit. Neither affects the work that follows the meeting,
/// which is what this story models. Materials are deferred for the same
/// reason — preparation does not remain.
/// </para>
/// </remarks>
public sealed class HaMeeting : AggregateRoot<HaMeetingId>
{
    public const int SubjectMaxLength = 300;
    public const int MinutesMaxLength = 8000;
    public const int OutcomeMaxLength = 2000;

    private readonly List<HaMeetingStatusEntry> _history = [];

    private HaMeeting()
    {
    }

    public TenantId TenantId { get; private set; } = default!;

    public AuthorityId AuthorityId { get; private set; }

    public string Subject { get; private set; } = default!;

    /// <summary>Type of meeting — FDA Type A/B/C, a scientific advice meeting.</summary>
    public AuthorityDivisionId? AuthorityDivisionId { get; private set; }

    public DateOnly? ScheduledFor { get; private set; }

    public RegulatoryApplicationId? RegulatoryApplicationId { get; private set; }

    public UserId? OwnerUserId { get; private set; }

    /// <summary>What was said. Recorded once it has been held.</summary>
    public string? Minutes { get; private set; }

    /// <summary>What the authority concluded. Not what we now owe.</summary>
    public string? Outcome { get; private set; }

    public HaMeetingStatus CurrentStatus { get; private set; }

    public IReadOnlyList<HaMeetingStatusEntry> History => _history.AsReadOnly();

    /// <summary>When it was first asked for, or first called. Derived.</summary>
    public DateOnly RaisedOn => _history[0].OccurredOn;

    /// <summary>When it actually happened, if it did. Derived.</summary>
    public DateOnly? HeldOn
        => _history
            .Where(x => x.Status == HaMeetingStatus.Held)
            .Select(x => (DateOnly?)x.OccurredOn)
            .FirstOrDefault();

    public static HaMeeting Begin(
        TenantId tenantId,
        AuthorityId authorityId,
        string subject,
        HaMeetingStatus initialStatus,
        DateOnly occurredOn,
        DateOnly? scheduledFor = null,
        AuthorityDivisionId? authorityDivisionId = null,
        RegulatoryApplicationId? regulatoryApplicationId = null,
        UserId? ownerUserId = null)
    {
        if (tenantId is null)
            throw new DomainException(HaMeetingErrors.TenantRequired);

        if (authorityId == default)
            throw new DomainException(HaMeetingErrors.AuthorityRequired);

        // Only two honest beginnings. A meeting cannot start Held, Declined or
        // Cancelled — those are things that happen to a meeting that exists.
        if (initialStatus is not (HaMeetingStatus.Requested or HaMeetingStatus.Granted))
            throw new DomainException(HaMeetingErrors.InvalidInitialStatus);

        var meeting = new HaMeeting
        {
            TenantId = tenantId,
            AuthorityId = authorityId,
            Subject = ValidatedSubject(subject),
            AuthorityDivisionId = authorityDivisionId,
            ScheduledFor = scheduledFor,
            RegulatoryApplicationId = regulatoryApplicationId,
            OwnerUserId = ownerUserId,
            CurrentStatus = initialStatus
        };

        meeting.Id = HaMeetingId.New();

        meeting._history.Add(new HaMeetingStatusEntry(
            HaMeetingStatusEntryId.New(),
            initialStatus,
            occurredOn,
            DateTime.UtcNow,
            null));

        return meeting;
    }

    public void ChangeStatus(HaMeetingStatus target, DateOnly occurredOn, string? note = null)
    {
        if (HaMeetingLifecycle.IsTerminal(CurrentStatus))
            throw new BusinessRuleViolationException(HaMeetingErrors.AlreadyConcluded);

        if (!HaMeetingLifecycle.Permits(CurrentStatus, target))
            throw new BusinessRuleViolationException(HaMeetingErrors.TransitionNotAllowed);

        if (occurredOn < _history[^1].OccurredOn)
            throw new DomainException(HaMeetingErrors.HistoryOutOfOrder);

        if (note is { Length: > HaMeetingStatusEntry.NoteMaxLength })
            throw new DomainException(HaMeetingErrors.NoteTooLong);

        _history.Add(new HaMeetingStatusEntry(
            HaMeetingStatusEntryId.New(),
            target,
            occurredOn,
            DateTime.UtcNow,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim()));

        CurrentStatus = target;
    }

    /// <summary>
    /// Records what was said and what the authority concluded. Only after the
    /// meeting happened — minutes of a meeting that has not taken place are a
    /// plan, and this aggregate does not hold plans.
    /// </summary>
    public void RecordOutcome(string? minutes, string? outcome)
    {
        if (CurrentStatus != HaMeetingStatus.Held)
            throw new BusinessRuleViolationException(HaMeetingErrors.OutcomeBeforeHeld);

        Minutes = Bounded(minutes, MinutesMaxLength, HaMeetingErrors.MinutesTooLong);
        Outcome = Bounded(outcome, OutcomeMaxLength, HaMeetingErrors.OutcomeTooLong);
    }

    public void Reschedule(DateOnly? scheduledFor) => ScheduledFor = scheduledFor;

    public void AssignTo(UserId? ownerUserId) => OwnerUserId = ownerUserId;

    private static string ValidatedSubject(string subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new DomainException(HaMeetingErrors.SubjectRequired);

        var trimmed = subject.Trim();

        if (trimmed.Length > SubjectMaxLength)
            throw new DomainException(HaMeetingErrors.SubjectTooLong);

        return trimmed;
    }

    private static string? Bounded(string? value, int max, string error)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        if (trimmed.Length > max)
            throw new DomainException(error);

        return trimmed;
    }
}
