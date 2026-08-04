using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.LocalLabels;

/// <summary>
/// A market's own controlled labelling document, and the revisions it has been
/// through.
/// </summary>
/// <remarks>
/// <b>A core label is the company's scientific position; this is a regulatory
/// artifact an authority approved</b>
/// ([ADR-059](../../../../../docs/adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md)).
/// The two are related and are not the same document — the authority regulates
/// this one.
/// <para>
/// <b>It carries no country.</b> It hangs off <see cref="MedicinalProductId"/>,
/// and the market-local tier already answers which jurisdiction that is
/// (ADR-039). A second copy could disagree with it.
/// </para>
/// <para>
/// <b>Carton artwork is a <see cref="LabelType"/>, not a separate aggregate</b>
/// (EPIC-018 D2). A printed carton is approved, revised and derived exactly as a
/// leaflet is, and giving it its own root would duplicate the revision logic,
/// the approval rules, the effective dating, the derivation, the API and the
/// browser proof to hold two or three extra columns. <b>The moment a rule reads
/// <c>if (Type == Artwork)</c>, that trade has stopped paying</b> — which is
/// what <c>LocalLabelTypeBranchTests</c> watches for.
/// </para>
/// </remarks>
public sealed class LocalLabel : AggregateRoot<LocalLabelId>
{
    private readonly List<LocalLabelRevision> _revisions = [];

    // Parameterless: LabelType is an owned value object, and EF cannot bind an
    // owned type to a constructor parameter. Enforced by
    // AggregateChildArchitectureTests.
    private LocalLabel()
    {
    }

    /// <summary>The owning tenant (ADR-031). Set once.</summary>
    public TenantId TenantId { get; private set; } = default!;

    /// <summary>
    /// The market this label belongs to. Immutable — repointing it would move
    /// an authority's approved document to a jurisdiction that never approved it.
    /// </summary>
    public MedicinalProductId MedicinalProductId { get; private set; } = default!;

    /// <summary>
    /// Prescribing information, patient leaflet, carton artwork, container
    /// label. Terminology, not a domain type: nothing branches on it, and every
    /// kind is revised identically.
    /// </summary>
    public CodedConcept LabelType { get; private set; } = default!;

    /// <summary>
    /// The language this market's document is written in — one per label, and
    /// a market with two languages holds two labels. Unlike a trade name, where
    /// language is a property of the name, here it is part of what the document
    /// <em>is</em>: a French carton and a Dutch carton are separately approved.
    /// </summary>
    public LanguageCode Language { get; private set; } = default!;

    public DateTime CreatedOnUtc { get; private set; }

    /// <summary>Every revision, oldest first once ordered.</summary>
    public IReadOnlyCollection<LocalLabelRevision> Revisions
        => _revisions.AsReadOnly();

    /// <summary>What this market's label says today, or null before the first
    /// approval.</summary>
    public LocalLabelRevision? RevisionInForce
        => _revisions.FirstOrDefault(x => x.IsInForce);

    /// <summary>The revision being prepared, if any. At most one.</summary>
    public LocalLabelRevision? Draft
        => _revisions.FirstOrDefault(
            x => x.Status == LocalLabelRevisionStatus.Draft);

    public static LocalLabel Create(
        TenantId tenantId,
        MedicinalProductId medicinalProductId,
        string? labelTypeCode,
        string? language,
        DateTime createdOnUtc)
    {
        if (tenantId is null)
            throw new DomainException(LocalLabelErrors.TenantRequired);

        if (medicinalProductId is null)
            throw new DomainException(
                LocalLabelErrors.MedicinalProductRequired);

        var label = new LocalLabel
        {
            Id = LocalLabelId.New(),
            TenantId = tenantId,
            MedicinalProductId = medicinalProductId,
            LabelType = ResolveType(labelTypeCode),
            Language = ResolveLanguage(language),
            CreatedOnUtc = createdOnUtc
        };

        // A label exists to hold revisions, so the first draft opens with it.
        label.StartRevision();

        return label;
    }

    /// <summary>
    /// Opens the next revision (N+1). The label owns numbering, and permits one
    /// open draft.
    /// </summary>
    public LocalLabelRevision StartRevision()
    {
        if (Draft is not null)
            throw new BusinessRuleViolationException(
                LocalLabelErrors.DraftAlreadyOpen);

        // Max, not Count + 1: a discarded draft's number is reissued, and a
        // number somebody has cited to an authority never is.
        var nextNumber = _revisions.Count == 0
            ? 1
            : _revisions.Max(x => x.RevisionNumber) + 1;

        var revision = new LocalLabelRevision(
            LocalLabelRevisionId.New(),
            nextNumber);

        _revisions.Add(revision);

        return revision;
    }

    /// <summary>
    /// Records everything settled while the revision is being prepared: the
    /// document, the core version it was written from, and the artwork code.
    /// </summary>
    public void PrepareRevision(
        LocalLabelRevisionId revisionId,
        ProductDocumentId? contentId,
        GlobalLabelVersionId? derivedFrom,
        string? dataCarrierCode,
        string? changeSummary)
        => RevisionOf(revisionId).Prepare(
            contentId, derivedFrom, dataCarrierCode, changeSummary);

    /// <summary>
    /// Puts a revision in force from a date, and retires the one it replaces.
    /// </summary>
    /// <remarks>
    /// One method, for the reason <c>GlobalLabel.PublishVersion</c> is one: a
    /// market with two approved labels in force is not a state a company can be
    /// in, and the supersede date is computed rather than supplied so the ranges
    /// meet exactly.
    /// </remarks>
    public void PublishRevision(
        LocalLabelRevisionId revisionId,
        DateOnly approvedOn,
        DateOnly effectiveFrom)
    {
        var revision = RevisionOf(revisionId);
        var current = RevisionInForce;

        if (current is not null
            && current.EffectiveFrom is { } from
            && effectiveFrom <= from)
        {
            throw new DomainException(
                LocalLabelErrors.EffectiveFromNotAfterRevisionInForce);
        }

        revision.Publish(approvedOn, effectiveFrom);

        current?.Supersede(effectiveFrom.AddDays(-1));
    }

    /// <summary>
    /// Throws away the open draft. Only ever a draft — an approved labelling
    /// document is a controlled record, and overwriting one is a governance
    /// failure rather than an edit.
    /// </summary>
    public void DiscardDraft()
    {
        var draft = Draft
            ?? throw new NotFoundException(LocalLabelErrors.NoOpenDraft);

        _revisions.Remove(draft);
    }

    private LocalLabelRevision RevisionOf(LocalLabelRevisionId revisionId)
        => _revisions.FirstOrDefault(x => x.Id == revisionId)
           ?? throw new NotFoundException(LocalLabelErrors.RevisionNotFound);

    /// <remarks>
    /// The vocabulary hands back a fresh instance per call — an owned coded
    /// value is tracked against exactly one owner (ADR-059 §7).
    /// </remarks>
    private static CodedConcept ResolveType(string? labelTypeCode)
    {
        if (string.IsNullOrWhiteSpace(labelTypeCode))
            throw new DomainException(LocalLabelErrors.LabelTypeRequired);

        return LabelVocabulary.LocalLabelTypeOf(labelTypeCode)
               ?? throw new DomainException(
                   LocalLabelErrors.LabelTypeNotRecognised);
    }

    private static LanguageCode ResolveLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new DomainException(LocalLabelErrors.LanguageRequired);

        return LanguageCode.FromIso639_1(language);
    }
}
