using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Domain.Aggregates.LocalLabels;

/// <summary>
/// One issue of a market's label — the controlled document an authority
/// approved, and the thing a regulator actually regulates.
/// </summary>
/// <remarks>
/// A child of <see cref="LocalLabel"/>: created only by it, numbered by it, and
/// every rule about it ("at most one draft", "at most one in force") is a
/// statement about the set. It carries no <c>TenantId</c> and is reachable only
/// through a filtered root (ADR-031).
/// <para>
/// <b>Its history is the market's, not the company's.</b> Japan may hold
/// fourteen revisions while the core label has had seven versions, may issue a
/// revision for a translation fix that changes nothing globally, and may adopt a
/// core version months after France did. None of that is derivable from the
/// global label, which is why it is stored here.
/// </para>
/// </remarks>
public sealed class LocalLabelRevision : Entity<LocalLabelRevisionId>
{
    public const int ChangeSummaryMaxLength = 2000;
    public const int DataCarrierCodeMaxLength = 100;

    // Parameterless: this entity owns no value object, but EF materialises it
    // alongside one that does, and a uniform shape here costs nothing.
    internal LocalLabelRevision(
        LocalLabelRevisionId id,
        int revisionNumber)
    {
        Id = id;
        RevisionNumber = revisionNumber;
        Status = LocalLabelRevisionStatus.Draft;
    }

    /// <summary>Assigned by the label. Never accepted from a caller.</summary>
    public int RevisionNumber { get; private set; }

    public LocalLabelRevisionStatus Status { get; private set; }

    /// <summary>The approved document itself (ADR-059 §6).</summary>
    public ProductDocumentId? ContentId { get; private set; }

    /// <summary>
    /// The core version this was written from. <b>Nullable, deliberately</b>
    /// (EPIC-018 D3): a migrated portfolio does not know which core version
    /// revision 9 came from, and a local-first company holds approved labelling
    /// before any core label exists here. Requiring it would force somebody to
    /// invent history.
    /// </summary>
    public GlobalLabelVersionId? DerivedFromGlobalLabelVersionId
    {
        get;
        private set;
    }

    /// <summary>
    /// Artwork's one identifying attribute — a barcode or data-matrix code.
    /// Null on every other type, which is the cost D2 accepted.
    /// </summary>
    /// <remarks>
    /// SKUs and pack configuration are deliberately absent: packaging is
    /// EPIC-010's, and this is the seam rather than a second packaging model.
    /// </remarks>
    public string? DataCarrierCode { get; private set; }

    public string? ChangeSummary { get; private set; }

    /// <summary>
    /// When the authority approved it. <b>Required before this revision can
    /// enter force</b> — a local label in force that no authority approved is a
    /// false statement about a regulated document.
    /// </summary>
    public DateOnly? ApprovedOn { get; private set; }

    /// <summary>
    /// When it takes effect in this market. A different fact from approval:
    /// <em>approved 12 May, effective 1 June</em> and <em>approved 12 May,
    /// effective immediately</em> both occur, and a model holding one date
    /// cannot say which happened.
    /// </summary>
    public DateOnly? EffectiveFrom { get; private set; }

    /// <summary>
    /// The last day it was in force, written by the label when a later revision
    /// supersedes it. Never supplied — the two dates must meet exactly.
    /// </summary>
    public DateOnly? EffectiveTo { get; private set; }

    public bool IsInForce => Status == LocalLabelRevisionStatus.InForce;

    /// <summary>
    /// Restates everything about the draft that is prepared before approval.
    /// </summary>
    /// <remarks>
    /// One method rather than four setters, and the same call the presentation
    /// uses: these facts are settled together while the revision is being
    /// prepared, and a caller able to change one without the others could point
    /// a Japanese translation of core v7 at a document that says v8.
    /// </remarks>
    internal void Prepare(
        ProductDocumentId? contentId,
        GlobalLabelVersionId? derivedFrom,
        string? dataCarrierCode,
        string? changeSummary)
    {
        RequireDraft();

        if (changeSummary is not null
            && changeSummary.Trim().Length > ChangeSummaryMaxLength)
        {
            throw new DomainException(LocalLabelErrors.ChangeSummaryTooLong);
        }

        if (dataCarrierCode is not null
            && dataCarrierCode.Trim().Length > DataCarrierCodeMaxLength)
        {
            throw new DomainException(LocalLabelErrors.DataCarrierCodeTooLong);
        }

        ContentId = contentId;
        DerivedFromGlobalLabelVersionId = derivedFrom;
        DataCarrierCode = Trimmed(dataCarrierCode);
        ChangeSummary = Trimmed(changeSummary);
    }

    internal void Publish(DateOnly approvedOn, DateOnly effectiveFrom)
    {
        RequireDraft();

        if (approvedOn == default)
            throw new DomainException(LocalLabelErrors.ApprovedOnRequired);

        if (effectiveFrom == default)
            throw new DomainException(LocalLabelErrors.EffectiveFromRequired);

        // The document is what the authority approved, so there must be one.
        if (ContentId is null)
            throw new BusinessRuleViolationException(
                LocalLabelErrors.ContentRequiredToPublish);

        // A label cannot take effect before the day it was approved. The two may
        // fall on the same day — "effective immediately" is ordinary — but a
        // label in force ahead of its own approval is not a state that exists.
        if (effectiveFrom < approvedOn)
            throw new DomainException(
                LocalLabelErrors.EffectiveBeforeApproval);

        Status = LocalLabelRevisionStatus.InForce;
        ApprovedOn = approvedOn;
        EffectiveFrom = effectiveFrom;
    }

    /// <param name="lastDayInForce">
    /// The day before its replacement takes effect, computed by the label.
    /// </param>
    internal void Supersede(DateOnly lastDayInForce)
    {
        Status = LocalLabelRevisionStatus.Superseded;
        EffectiveTo = lastDayInForce;
    }

    private void RequireDraft()
    {
        if (Status != LocalLabelRevisionStatus.Draft)
            throw new BusinessRuleViolationException(
                LocalLabelErrors.RevisionNotDraft);
    }

    private static string? Trimmed(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
