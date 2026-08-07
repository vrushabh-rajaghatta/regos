using RegOS.Platform.Contracts;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Aggregates.ProcessObjectives;

/// <summary>
/// What we are trying to achieve in one market, and why.
/// </summary>
/// <remarks>
/// <b>An objective is the goal; plans are attempts</b>
/// ([ADR-065 decision 3](../../../../../docs/adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md)).
/// It is a separate aggregate from <c>ProcessPlan</c> — they are 1:1 today and
/// conceptually are not — on the demonstration that an objective is stateable
/// with no schedule under it at all: <em>FDA approval for Product X</em>,
/// <em>CE MDR transition</em>, <em>expand an indication</em>, <em>renew a
/// licence</em>. Each is nameable, ownable and reportable before anyone has
/// scheduled anything.
/// <para>
/// <b>It targets a global product in a country, never a
/// <c>MedicinalProduct</c></b> (ADR-065 D8). The deciding question is when the
/// business first knows the objective exists, and the answer is almost always
/// <em>before the market-local record does</em> — <em>"file in Japan in
/// FY2028"</em> is a real objective on a market RegOS holds no regulatory record
/// for. Requiring one would force an organisation to create a regulatory
/// artefact purely to satisfy a foreign key.
/// </para>
/// <para>
/// <b>Its lifecycle is one of intent, not of execution.</b> A plan slipping does
/// not move an objective; abandoning the goal does.
/// </para>
/// </remarks>
public sealed class ProcessObjective : AggregateRoot<ProcessObjectiveId>
{
    public const int NameMaxLength = 300;
    public const int RationaleMaxLength = 4000;

    private readonly List<ProcessObjectiveStatusEntry> _history = [];

    // EF materialisation.
    private ProcessObjective()
    {
    }

    /// <summary>
    /// The owning tenant. <b>Fail-closed</b>, unlike <c>ProcessDefinition</c>:
    /// a playbook is knowledge RegOS can ship, and an objective is a company's
    /// own strategy, which nothing shared could ever be.
    /// </summary>
    public TenantId TenantId { get; private set; } = default!;

    /// <summary>What the objective is about. Immutable.</summary>
    public GlobalProductId GlobalProductId { get; private set; } = default!;

    /// <summary>Which market. Immutable — a different market is a different goal.</summary>
    public CountryId CountryId { get; private set; }

    /// <summary>
    /// The market-local record that fulfils this objective, once one exists.
    /// </summary>
    /// <remarks>
    /// <b>Nullable because the market record does not exist yet, not because the
    /// objective is incomplete</b> — the honest representation of the timeline
    /// (ADR-065 D8):
    /// <code>
    /// Global Product → Objective (product + country) → market preparation
    ///                                                        ↓
    ///      objective optionally linked ← MedicinalProduct created
    /// </code>
    /// See <see cref="ConfirmMarketRecord"/> for the invariant it carries and
    /// where that invariant is enforced.
    /// </remarks>
    public MedicinalProductId? MedicinalProductId { get; private set; }

    /// <summary>
    /// The vehicle, when one has been chosen. RIM draws this as <em>Peer,
    /// Conditional</em> and so does RegOS: an objective is <em>"get approved in
    /// Japan"</em>, an application is how, and one objective may run through
    /// several over years.
    /// </summary>
    public RegulatoryApplicationId? RegulatoryApplicationId { get; private set; }

    public string Name { get; private set; } = default!;

    /// <summary>
    /// <b>Why this, and why this route.</b> The strategy content — and the reason
    /// this aggregate is separate from the plan rather than a field on it.
    /// </summary>
    public string? Rationale { get; private set; }

    /// <summary>One of ours (ADR-041). Nullable: an unowned objective is normal
    /// while it is still proposed.</summary>
    public UserId? OwnerUserId { get; private set; }

    /// <summary>When we want it by. An intention, not a schedule — a plan holds
    /// the schedule.</summary>
    public DateOnly? TargetCompletionOn { get; private set; }

    /// <summary>
    /// Stored, and it earns that by a query: the objectives list filters on
    /// "not achieved and not abandoned" across a whole tenant, which would
    /// otherwise walk every history.
    /// </summary>
    public ProcessObjectiveStatus CurrentStatus { get; private set; }

    public IReadOnlyList<ProcessObjectiveStatusEntry> History => _history.AsReadOnly();

    /// <summary>When it was first stated — the first entry's business date.</summary>
    public DateOnly StatedOn => _history[0].OccurredOn;

    /// <summary>When we got what we were after, if we have. Derived, never stored.</summary>
    public DateOnly? AchievedOn
        => _history
            .Where(x => x.Status == ProcessObjectiveStatus.Achieved)
            .Select(x => (DateOnly?)x.OccurredOn)
            .FirstOrDefault();

    public static ProcessObjective Create(
        TenantId tenantId,
        GlobalProductId globalProductId,
        CountryId countryId,
        string name,
        DateOnly statedOn,
        string? rationale = null,
        UserId? ownerUserId = null,
        DateOnly? targetCompletionOn = null)
    {
        if (tenantId is null)
            throw new DomainException(ProcessObjectiveErrors.TenantRequired);

        if (globalProductId is null)
            throw new DomainException(ProcessObjectiveErrors.ProductRequired);

        if (countryId == default)
            throw new DomainException(ProcessObjectiveErrors.CountryRequired);

        var objective = new ProcessObjective
        {
            Id = ProcessObjectiveId.New(),
            TenantId = tenantId,
            GlobalProductId = globalProductId,
            CountryId = countryId,
            Name = ValidatedName(name),
            Rationale = ValidatedRationale(rationale),
            OwnerUserId = ownerUserId,
            TargetCompletionOn = targetCompletionOn,
            CurrentStatus = ProcessObjectiveStatus.Proposed
        };

        objective._history.Add(new ProcessObjectiveStatusEntry(
            ProcessObjectiveStatusEntryId.New(),
            ProcessObjectiveStatus.Proposed,
            statedOn,
            DateTime.UtcNow,
            null));

        return objective;
    }

    public void ChangeStatus(
        ProcessObjectiveStatus target, DateOnly occurredOn, string? note = null)
    {
        if (CurrentStatus is ProcessObjectiveStatus.Achieved
            or ProcessObjectiveStatus.Abandoned)
            throw new BusinessRuleViolationException(
                ProcessObjectiveErrors.AlreadyClosed);

        if (target == ProcessObjectiveStatus.Proposed)
            throw new BusinessRuleViolationException(
                ProcessObjectiveErrors.CannotReturnToProposed);

        if (target == CurrentStatus)
            throw new BusinessRuleViolationException(
                ProcessObjectiveErrors.AlreadyInThatStatus);

        // The chronology rule — Max, not [^1]: this collection is loaded through
        // an unordered Include, so its last element is the last row the database
        // returned rather than the latest event.
        if (occurredOn < _history.Max(entry => entry.OccurredOn))
            throw new DomainException(ProcessObjectiveErrors.HistoryOutOfOrder);

        if (note is { Length: > ProcessObjectiveStatusEntry.NoteMaxLength })
            throw new DomainException(ProcessObjectiveErrors.NoteTooLong);

        _history.Add(new ProcessObjectiveStatusEntry(
            ProcessObjectiveStatusEntryId.New(),
            target,
            occurredOn,
            DateTime.UtcNow,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim()));

        CurrentStatus = target;
    }

    /// <summary>
    /// Records which market-local product fulfils this objective — or clears it.
    /// </summary>
    /// <remarks>
    /// <b>The link confirms identity; it never redefines it.</b> A populated
    /// <see cref="MedicinalProductId"/> must reference a record whose global
    /// product and country are the ones this objective already holds. Attaching a
    /// US market record to a Japan objective is a domain error, and that rule is
    /// what stops the duplicated pair drifting.
    /// <para>
    /// <b>The rule is real and this is not where it lives.</b> Checking it means
    /// loading a <c>MedicinalProduct</c>, which is the cross-aggregate read
    /// [ADR-016](../../../../../docs/adr/ADR-016-persistence-access-model.md)
    /// keeps out of the domain — <b>the command handler resolves and verifies the
    /// record before it gets here</b>. <c>LocalLabel.PrintedFor</c> documents the
    /// identical situation for packs and EPIC-010b resolved it the same way.
    /// </para>
    /// </remarks>
    public void ConfirmMarketRecord(MedicinalProductId? medicinalProductId)
        => MedicinalProductId = medicinalProductId;

    /// <summary>Names the application this objective is being pursued through.</summary>
    public void PursueThrough(RegulatoryApplicationId? regulatoryApplicationId)
        => RegulatoryApplicationId = regulatoryApplicationId;

    public void AssignTo(UserId? ownerUserId) => OwnerUserId = ownerUserId;

    public void Amend(string name, string? rationale, DateOnly? targetCompletionOn)
    {
        Name = ValidatedName(name);
        Rationale = ValidatedRationale(rationale);
        TargetCompletionOn = targetCompletionOn;
    }

    private static string ValidatedName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ProcessObjectiveErrors.NameRequired);

        var trimmed = name.Trim();

        return trimmed.Length > NameMaxLength
            ? throw new DomainException(ProcessObjectiveErrors.NameTooLong)
            : trimmed;
    }

    private static string? ValidatedRationale(string? rationale)
    {
        if (string.IsNullOrWhiteSpace(rationale))
            return null;

        var trimmed = rationale.Trim();

        return trimmed.Length > RationaleMaxLength
            ? throw new DomainException(ProcessObjectiveErrors.RationaleTooLong)
            : trimmed;
    }
}
