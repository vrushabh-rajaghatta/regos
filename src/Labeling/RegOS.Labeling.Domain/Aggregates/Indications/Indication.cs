using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.Indications;

/// <summary>
/// What this product is approved to treat in one market.
/// </summary>
/// <remarks>
/// <b>A regulatory fact, not an editorial artifact</b>
/// ([ADR-059](../../../../../docs/adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md)).
/// It hangs off <see cref="MedicinalProductId"/> because an indication is
/// approved <em>for a product in a market</em>; the label publishes it, and
/// nothing here points at a label version — see ADR-059 §3 for the five
/// questions that link would have to answer first.
/// <para>
/// <b>It has a dated history and no revisions.</b> Approved, expanded,
/// restricted, withdrawn are successive regulatory <em>decisions</em>, not
/// successive editions of a document. The discriminator that settled it: for a
/// label the wording is the regulated object; here the approval is, and the
/// wording is how the label communicates it.
/// </para>
/// <para>
/// <b>The condition is coded and the text is not, and both are needed.</b> The
/// code is what makes <em>"which markets approve indication X?"</em> a question
/// at all — <em>Type 2 diabetes mellitus</em>, <em>Diabète sucré de type 2</em>
/// and <em>Diabetes mellitus Typ 2</em> are one concept in three markets, and
/// free text cannot say so. The text is what the approved local label actually
/// says. Same split as <c>Ingredient</c>: a coded substance, and a strength
/// stated beside it.
/// </para>
/// </remarks>
public sealed class Indication : AggregateRoot<IndicationId>
{
    public const int LabelTextMaxLength = 4000;

    private readonly List<Population> _populations = [];
    private readonly List<OtherTherapy> _otherTherapies = [];
    private readonly List<IndicationStatusEntry> _statusHistory = [];

    private Indication()
    {
    }

    /// <summary>The owning tenant (ADR-031). Set once.</summary>
    public TenantId TenantId { get; private set; } = default!;

    /// <summary>
    /// The market this is approved in. Immutable — repointing it would move one
    /// authority's decision to a jurisdiction that never took it.
    /// </summary>
    public MedicinalProductId MedicinalProductId { get; private set; } = default!;

    /// <summary>
    /// The clinical concept, from <see cref="ClinicalConditionVocabulary"/>.
    /// The join key for every cross-market question.
    /// </summary>
    public CodedConcept Condition { get; private set; } = default!;

    /// <summary>What the approved local label says, in the market's own words.</summary>
    public string LabelText { get; private set; } = default!;

    /// <summary>
    /// Where this authorisation currently stands. Stored, not replayed — the
    /// portfolio views read one indexed column rather than reducing a history
    /// per row. The same shape <c>MedicinalProduct</c> uses for market status.
    /// </summary>
    public IndicationStatus CurrentStatus { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    /// <summary>Every decision this authorisation has been through, oldest first.</summary>
    public IReadOnlyCollection<IndicationStatusEntry> StatusHistory
        => _statusHistory.AsReadOnly();

    /// <summary>Who it applies to. Empty means everyone, which is ordinary.</summary>
    public IReadOnlyCollection<Population> Populations
        => _populations.AsReadOnly();

    public IReadOnlyCollection<OtherTherapy> OtherTherapies
        => _otherTherapies.AsReadOnly();

    public static Indication Record(
        TenantId tenantId,
        MedicinalProductId medicinalProductId,
        string? conditionCode,
        string? labelText,
        DateOnly approvedOn,
        DateTime createdOnUtc)
    {
        if (tenantId is null)
            throw new DomainException(IndicationErrors.TenantRequired);

        if (medicinalProductId is null)
            throw new DomainException(
                IndicationErrors.MedicinalProductRequired);

        if (approvedOn == default)
            throw new DomainException(IndicationErrors.OccurredOnRequired);

        var indication = new Indication
        {
            Id = IndicationId.New(),
            TenantId = tenantId,
            MedicinalProductId = medicinalProductId,
            Condition = ResolveCondition(conditionCode),
            LabelText = NormalizeText(labelText),
            CreatedOnUtc = createdOnUtc
        };

        // The first entry is the decision it starts in, not a separate
        // "created" event: the history is one chronological sequence of the
        // decisions taken, in one vocabulary. The shape MedicinalProduct uses.
        indication.Append(IndicationStatus.Approved, approvedOn, null);

        return indication;
    }

    /// <summary>
    /// Restates what the label says, without touching the authorisation.
    /// </summary>
    /// <remarks>
    /// Wording and authorisation move independently, which is the whole reason
    /// this aggregate has no revisions: a translation corrected in the label is
    /// a <c>LocalLabelRevision</c>, and it changes nothing about what the
    /// authority approved.
    /// </remarks>
    public void RestateLabelText(string? labelText)
        => LabelText = NormalizeText(labelText);

    /// <summary>
    /// Records what an authority decided, and when.
    /// </summary>
    /// <remarks>
    /// <b>There is no transition table.</b> An indication may be restricted,
    /// later expanded again, and withdrawn years after that; none of that is
    /// incoherent. Two rules survive because they are about coherence rather
    /// than process: a decision cannot re-state the one already in force, and
    /// business time moves forward.
    /// </remarks>
    public void RecordDecision(
        IndicationStatus status,
        DateOnly occurredOn,
        string? note = null)
    {
        if (!Enum.IsDefined(status))
            throw new DomainException(IndicationErrors.StatusNotRecognised);

        if (occurredOn == default)
            throw new DomainException(IndicationErrors.OccurredOnRequired);

        if (status == CurrentStatus)
            throw new BusinessRuleViolationException(
                IndicationErrors.AlreadyInStatus(status));

        // Max, not [^1]: nothing orders the collection the database hands back,
        // and an invariant may not depend on the order its own storage returns
        // things in. The lesson MedicinalProduct's history paid for.
        if (occurredOn < _statusHistory.Max(entry => entry.OccurredOn))
            throw new DomainException(
                IndicationErrors.OccurredOnBeforePreviousEntry);

        Append(status, occurredOn, note);
    }

    public Population AddPopulation(
        int? ageLow,
        int? ageHigh,
        string? ageUnitCode,
        string? genderCode,
        string? physiologicalConditionCode,
        string? description)
    {
        var population = Population.Create(
            ageLow, ageHigh, ageUnitCode, genderCode,
            physiologicalConditionCode, description);

        _populations.Add(population);

        return population;
    }

    /// <summary>
    /// Corrects a qualifier in place — <b>the operation that justifies
    /// <see cref="Population"/> having identity at all</b> (EPIC-018 D2).
    /// </summary>
    public void AmendPopulation(
        PopulationId populationId,
        int? ageLow,
        int? ageHigh,
        string? ageUnitCode,
        string? genderCode,
        string? physiologicalConditionCode,
        string? description)
        => PopulationOf(populationId).Amend(
            ageLow, ageHigh, ageUnitCode, genderCode,
            physiologicalConditionCode, description);

    /// <remarks>
    /// Removal, not retirement. A population qualifier has no lifecycle of its
    /// own — it is part of the statement as it currently stands, and the
    /// regulatory history lives in <see cref="StatusHistory"/> where the
    /// decisions are. A qualifier recorded in error is a mistake to correct,
    /// not a fact to preserve.
    /// </remarks>
    public void RemovePopulation(PopulationId populationId)
        => _populations.Remove(PopulationOf(populationId));

    public OtherTherapy AddOtherTherapy(
        string? relationshipCode,
        string? therapy)
    {
        var otherTherapy = OtherTherapy.Create(relationshipCode, therapy);

        _otherTherapies.Add(otherTherapy);

        return otherTherapy;
    }

    public void RemoveOtherTherapy(OtherTherapyId otherTherapyId)
    {
        var therapy = _otherTherapies.FirstOrDefault(x => x.Id == otherTherapyId)
            ?? throw new NotFoundException(
                IndicationErrors.OtherTherapyNotFound);

        _otherTherapies.Remove(therapy);
    }

    /// <summary>
    /// The core invariant: <b>every decision updates
    /// <see cref="CurrentStatus"/> and appends exactly one immutable entry.</b>
    /// Nothing else writes either, so the current authorisation and the record
    /// of how it was reached can never disagree.
    /// </summary>
    private void Append(
        IndicationStatus status,
        DateOnly occurredOn,
        string? note)
    {
        if (note is not null
            && note.Trim().Length > IndicationStatusEntry.NoteMaxLength)
        {
            throw new DomainException(IndicationErrors.NoteTooLong);
        }

        CurrentStatus = status;

        _statusHistory.Add(new IndicationStatusEntry(
            IndicationStatusEntryId.New(),
            status,
            occurredOn,
            DateTime.UtcNow,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim()));
    }

    private Population PopulationOf(PopulationId populationId)
        => _populations.FirstOrDefault(x => x.Id == populationId)
           ?? throw new NotFoundException(IndicationErrors.PopulationNotFound);

    private static string NormalizeText(string? labelText)
    {
        if (string.IsNullOrWhiteSpace(labelText))
            throw new DomainException(IndicationErrors.LabelTextRequired);

        var trimmed = labelText.Trim();

        return trimmed.Length > LabelTextMaxLength
            ? throw new DomainException(IndicationErrors.LabelTextTooLong)
            : trimmed;
    }

    private static CodedConcept ResolveCondition(string? conditionCode)
    {
        if (string.IsNullOrWhiteSpace(conditionCode))
            throw new DomainException(IndicationErrors.ConditionRequired);

        return ClinicalConditionVocabulary.ConditionOf(conditionCode)
               ?? throw new DomainException(
                   IndicationErrors.ConditionNotRecognised);
    }
}
