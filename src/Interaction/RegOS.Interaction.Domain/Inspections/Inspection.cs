using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Platform.Contracts;
using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Interaction.Domain.Inspections;

/// <summary>
/// An authority's inspection of a site.
/// </summary>
/// <remarks>
/// <b>The site is constitutive, not metadata.</b> An inspection is not a record
/// that happens to have a location — <em>the authority physically went
/// somewhere</em>, and that somewhere is what was inspected. It is very often a
/// contract manufacturer's site rather than ours, which is exactly why
/// <c>OrganizationSite</c> is a root with a cross-organization directory
/// (ADR-038). This is that root earning itself from a direction ADR-038 did not
/// predict.
/// <para>
/// Nullable all the same: <em>"the FDA will inspect us in March"</em> arrives
/// before anyone knows which of three plants, and forcing the answer early would
/// mean inventing an unknown site or delaying the record. Naming the site later
/// is its own business event.
/// </para>
/// <para>
/// <b>There is no observation entity, and the reason is not brevity.</b> A
/// Form 483 observation looks like an <c>HaQuestion</c> — numbered, texted,
/// responded to — and is a different kind of thing. A question asks for
/// information, and answering it <em>is</em> the work. An observation asserts a
/// deficiency, and responding to it <em>creates</em> work: a corrective action,
/// which is a <c>Commitment</c> that already exists. Modelling observations
/// would add an object whose only purpose is to produce commitments.
/// </para>
/// <para>
/// So <see cref="Outcome"/> holds the authority's findings — their position,
/// scannable — and what those findings oblige lives on <c>Commitment</c>, with
/// its own due date, owner and history. Same split as <c>HaMeeting</c>.
/// </para>
/// <para>
/// <b>An inspection concludes.</b> It belongs to the same family as a meeting
/// rather than to correspondence, questions and commitments: its value is the
/// work it produces, not a continuing lifecycle.
/// </para>
/// </remarks>
public sealed class Inspection : AggregateRoot<InspectionId>
{
    public const int TitleMaxLength = 300;
    public const int OutcomeMaxLength = 8000;

    private readonly List<InspectionStatusEntry> _history = [];

    private Inspection()
    {
    }

    public TenantId TenantId { get; private set; } = default!;

    public AuthorityId AuthorityId { get; private set; }

    public string Title { get; private set; } = default!;

    /// <summary>What was inspected. Nullable until it is known.</summary>
    public OrganizationSiteId? OrganizationSiteId { get; private set; }

    public DateOnly? ScheduledFor { get; private set; }

    public UserId? OwnerUserId { get; private set; }

    /// <summary>The planned work this inspection serves, if any.</summary>
    /// <remarks>
    /// Rarest of the four, and kept for the same reason the others are: an
    /// inspection is occasionally a condition of a filing, and when it is, the
    /// plan should be able to say so. When it is not — the ordinary case, an
    /// authority arriving on its own schedule — the null means nothing (I9).
    /// </remarks>
    public ProcessStepId? ProcessStepId { get; private set; }

    /// <summary>What the authority found. Not what we must now do.</summary>
    public string? Outcome { get; private set; }

    public InspectionStatus CurrentStatus { get; private set; }

    public IReadOnlyList<InspectionStatusEntry> History => _history.AsReadOnly();

    /// <summary>When we first learned of it. Derived.</summary>
    public DateOnly RaisedOn => _history[0].OccurredOn;

    /// <summary>When it finished, if it did. Derived.</summary>
    public DateOnly? CompletedOn
        => _history
            .Where(x => x.Status == InspectionStatus.Completed)
            .Select(x => (DateOnly?)x.OccurredOn)
            .FirstOrDefault();

    public static Inspection Begin(
        TenantId tenantId,
        AuthorityId authorityId,
        string title,
        InspectionStatus initialStatus,
        DateOnly occurredOn,
        OrganizationSiteId? organizationSiteId = null,
        DateOnly? scheduledFor = null,
        UserId? ownerUserId = null)
    {
        if (tenantId is null)
            throw new DomainException(InspectionErrors.TenantRequired);

        if (authorityId == default)
            throw new DomainException(InspectionErrors.AuthorityRequired);

        if (initialStatus is not (InspectionStatus.Announced or InspectionStatus.InProgress))
            throw new DomainException(InspectionErrors.InvalidInitialStatus);

        var inspection = new Inspection
        {
            TenantId = tenantId,
            AuthorityId = authorityId,
            Title = ValidatedTitle(title),
            OrganizationSiteId = organizationSiteId,
            ScheduledFor = scheduledFor,
            OwnerUserId = ownerUserId,
            CurrentStatus = initialStatus
        };

        inspection.Id = InspectionId.New();

        inspection._history.Add(new InspectionStatusEntry(
            InspectionStatusEntryId.New(),
            initialStatus,
            occurredOn,
            DateTime.UtcNow,
            null));

        return inspection;
    }

    public void ChangeStatus(InspectionStatus target, DateOnly occurredOn, string? note = null)
    {
        if (CurrentStatus is InspectionStatus.Completed or InspectionStatus.Cancelled)
            throw new BusinessRuleViolationException(InspectionErrors.AlreadyConcluded);

        if (target == InspectionStatus.Announced)
            throw new BusinessRuleViolationException(
                InspectionErrors.CannotReturnToAnnounced);

        if (target == CurrentStatus)
            throw new BusinessRuleViolationException(InspectionErrors.AlreadyInThatStatus);

        // Max, not [^1]: this collection is loaded through an unordered Include,
        // so its last element is the last row the database returned rather than
        // the latest event. The rule has to read the history, not the result set.
        if (occurredOn < _history.Max(entry => entry.OccurredOn))
            throw new DomainException(InspectionErrors.HistoryOutOfOrder);

        if (note is { Length: > InspectionStatusEntry.NoteMaxLength })
            throw new DomainException(InspectionErrors.NoteTooLong);

        _history.Add(new InspectionStatusEntry(
            InspectionStatusEntryId.New(),
            target,
            occurredOn,
            DateTime.UtcNow,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim()));

        CurrentStatus = target;
    }

    /// <summary>
    /// Records what the authority found. Only once it has completed —
    /// findings before an inspection finishes are a guess.
    /// </summary>
    public void RecordFindings(string? outcome)
    {
        if (CurrentStatus != InspectionStatus.Completed)
            throw new BusinessRuleViolationException(
                InspectionErrors.OutcomeBeforeCompleted);

        if (string.IsNullOrWhiteSpace(outcome))
        {
            Outcome = null;
            return;
        }

        var trimmed = outcome.Trim();

        if (trimmed.Length > OutcomeMaxLength)
            throw new DomainException(InspectionErrors.OutcomeTooLong);

        Outcome = trimmed;
    }

    /// <summary>Naming the site once it is known is its own business event.</summary>
    public void InspectedAt(OrganizationSiteId? organizationSiteId)
        => OrganizationSiteId = organizationSiteId;

    /// <summary>
    /// Records which planned work this inspection serves, or clears it.
    /// Changes discoverability and nothing else.
    /// </summary>
    public void AttachToStep(ProcessStepId? processStepId)
        => ProcessStepId = processStepId;

    public void AssignTo(UserId? ownerUserId) => OwnerUserId = ownerUserId;

    private static string ValidatedTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException(InspectionErrors.TitleRequired);

        var trimmed = title.Trim();

        if (trimmed.Length > TitleMaxLength)
            throw new DomainException(InspectionErrors.TitleTooLong);

        return trimmed;
    }
}
