using RegOS.Interaction.Domain.Correspondence;
using RegOS.Interaction.Domain.Inspections;
using RegOS.Interaction.Domain.Meetings;
using RegOS.Platform.Contracts;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Interaction.Domain.Commitments;

/// <summary>
/// Something we promised a health authority we would do, by a date.
/// </summary>
/// <remarks>
/// <b>A root, and the archetype is not an answer to a question.</b> It is the
/// post-marketing commitment: an approval letter carries conditions — run this
/// study, submit this data by that date — which are terms of approval rather
/// than replies. Those outlive the interaction that created them, often by
/// years, and are tracked long after the letter is closed.
/// <para>
/// <b>The authority is intrinsic, not inherited</b> (ADR-040 decision 3). A
/// commitment is <em>made to</em> an authority; a commitment with no source
/// letter is ordinary, and one with no authority is meaningless. Storing it
/// therefore does not duplicate a parent's fact, because there is no required
/// parent.
/// </para>
/// <para>
/// <b><see cref="GivenOn"/> is derived, never stored.</b> It is
/// <c>history[0].OccurredOn</c> — the date the commitment was made is the date
/// its first status began. A stored copy could disagree with the history beside
/// it, which is the same call <c>LaunchedOn</c> and <c>RespondedOn</c> made
/// (ADR-037).
/// </para>
/// <para>
/// <b><see cref="CurrentStatus"/> is stored, and earns it by a query.</b> The
/// due view asks for commitments that are neither fulfilled nor waived, across
/// the whole tenant — a filter that would otherwise walk every history.
/// </para>
/// </remarks>
public sealed class Commitment : AggregateRoot<CommitmentId>
{
    public const int TitleMaxLength = 300;
    public const int DescriptionMaxLength = 4000;

    private readonly List<CommitmentStatusEntry> _history = [];

    private Commitment()
    {
    }

    public TenantId TenantId { get; private set; } = default!;

    /// <summary>The authority we promised. Immutable.</summary>
    public AuthorityId AuthorityId { get; private set; }

    public string Title { get; private set; } = default!;

    public string? Description { get; private set; }

    /// <summary>When we said we would have it done.</summary>
    public DateOnly DueOn { get; private set; }

    /// <summary>
    /// One of ours (ADR-041). Nullable: an unassigned commitment is the normal
    /// state of one that has just arrived in an approval letter.
    /// </summary>
    public UserId? OwnerUserId { get; private set; }

    /// <summary>What it is about.</summary>
    public RegistrationId? RegistrationId { get; private set; }

    public RegulatoryApplicationId? RegulatoryApplicationId { get; private set; }

    /// <summary>Where it arose, when it arose from a letter.</summary>
    public HaCorrespondenceId? SourceCorrespondenceId { get; private set; }

    /// <summary>
    /// Where it arose, when it arose at a meeting. Deferred in S004 because a
    /// column for a type that did not exist yet would have been the
    /// speculation this epic keeps refusing; it arrived with S005.
    /// </summary>
    public HaMeetingId? SourceMeetingId { get; private set; }

    /// <summary>
    /// Where it arose, when it arose from an inspection's findings — a
    /// corrective action.
    /// </summary>
    /// <remarks>
    /// The <b>third</b> independent business origin. Three nullable foreign
    /// keys are referentially enforced, individually queryable and honest,
    /// which a polymorphic <c>(SourceType, SourceId)</c> pair is not. But this
    /// is the simplest truthful model measured, not an ideal one: <b>a fourth
    /// independent business origin reopens the Phase 2 ownership discussion</b>
    /// — the one that falsified an <c>AuthorityInteraction</c> supertype.
    /// Deliberately worded as origins rather than columns, so adding a
    /// <c>MeetingFollowUpId</c> does not trip it: that is still a
    /// meeting-derived commitment.
    /// </remarks>
    public InspectionId? SourceInspectionId { get; private set; }

    public CommitmentStatus CurrentStatus { get; private set; }

    public IReadOnlyList<CommitmentStatusEntry> History => _history.AsReadOnly();

    /// <summary>The date we made the promise — the first entry's business date.</summary>
    public DateOnly GivenOn => _history[0].OccurredOn;

    /// <summary>When we discharged it, if we have. Derived from the history.</summary>
    public DateOnly? FulfilledOn
        => _history
            .Where(x => x.Status == CommitmentStatus.Fulfilled)
            .Select(x => (DateOnly?)x.OccurredOn)
            .FirstOrDefault();

    public static Commitment Give(
        TenantId tenantId,
        AuthorityId authorityId,
        string title,
        DateOnly givenOn,
        DateOnly dueOn,
        string? description = null,
        UserId? ownerUserId = null,
        RegistrationId? registrationId = null,
        RegulatoryApplicationId? regulatoryApplicationId = null,
        HaCorrespondenceId? sourceCorrespondenceId = null,
        HaMeetingId? sourceMeetingId = null,
        InspectionId? sourceInspectionId = null)
    {
        if (tenantId is null)
            throw new DomainException(CommitmentErrors.TenantRequired);

        if (authorityId == default)
            throw new DomainException(CommitmentErrors.AuthorityRequired);

        var commitment = new Commitment
        {
            TenantId = tenantId,
            AuthorityId = authorityId,
            Title = ValidatedTitle(title),
            Description = ValidatedDescription(description),
            DueOn = dueOn,
            OwnerUserId = ownerUserId,
            RegistrationId = registrationId,
            RegulatoryApplicationId = regulatoryApplicationId,
            SourceCorrespondenceId = sourceCorrespondenceId,
            SourceMeetingId = sourceMeetingId,
            SourceInspectionId = sourceInspectionId,
            CurrentStatus = CommitmentStatus.Open
        };

        commitment.Id = CommitmentId.New();

        commitment._history.Add(new CommitmentStatusEntry(
            CommitmentStatusEntryId.New(),
            CommitmentStatus.Open,
            givenOn,
            DateTime.UtcNow,
            null));

        return commitment;
    }

    public void ChangeStatus(CommitmentStatus target, DateOnly occurredOn, string? note = null)
    {
        if (CurrentStatus is CommitmentStatus.Fulfilled or CommitmentStatus.Waived)
            throw new BusinessRuleViolationException(CommitmentErrors.AlreadyClosed);

        if (target == CommitmentStatus.Open)
            throw new BusinessRuleViolationException(CommitmentErrors.CannotReopen);

        if (target == CurrentStatus)
            throw new BusinessRuleViolationException(CommitmentErrors.AlreadyInThatStatus);

        // The chronology rule — one line, and the fourth copy of it. S003
        // measured that it is not worth extracting.
        if (occurredOn < _history[^1].OccurredOn)
            throw new DomainException(CommitmentErrors.HistoryOutOfOrder);

        if (note is { Length: > CommitmentStatusEntry.NoteMaxLength })
            throw new DomainException(CommitmentErrors.NoteTooLong);

        _history.Add(new CommitmentStatusEntry(
            CommitmentStatusEntryId.New(),
            target,
            occurredOn,
            DateTime.UtcNow,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim()));

        CurrentStatus = target;
    }

    public void AssignTo(UserId? ownerUserId) => OwnerUserId = ownerUserId;

    public void Amend(string title, string? description, DateOnly dueOn)
    {
        Title = ValidatedTitle(title);
        Description = ValidatedDescription(description);
        DueOn = dueOn;
    }

    private static string ValidatedTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException(CommitmentErrors.TitleRequired);

        var trimmed = title.Trim();

        if (trimmed.Length > TitleMaxLength)
            throw new DomainException(CommitmentErrors.TitleTooLong);

        return trimmed;
    }

    private static string? ValidatedDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var trimmed = description.Trim();

        if (trimmed.Length > DescriptionMaxLength)
            throw new DomainException(CommitmentErrors.DescriptionTooLong);

        return trimmed;
    }
}
