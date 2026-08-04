using RegOS.ProductDocument.Domain.IDs;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Domain.Aggregates.GlobalLabels;

/// <summary>
/// One issue of a global label — the thing that is actually in force on a date.
/// </summary>
/// <remarks>
/// A child entity of <see cref="GlobalLabel"/>, not a root: it is created only
/// by its label, its number is assigned by its label, and every rule about it
/// ("at most one draft", "at most one in force") is a statement about the set
/// rather than about one member. It carries no <c>TenantId</c> — it is reachable
/// only through a filtered root (ADR-031).
/// <para>
/// <b>The content link is a <c>ProductDocumentId</c> and nothing more</b>
/// (ADR-059 §6). The document stores the file; this says what that file
/// <em>is</em> — version 3 of the core data sheet, in force from June. Nothing
/// about a market lives here, because a global label has no market; that is
/// <c>LocalLabel</c>'s job in S002.
/// </para>
/// </remarks>
public sealed class GlobalLabelVersion : Entity<GlobalLabelVersionId>
{
    public const int ChangeSummaryMaxLength = 2000;

    // EF binds by constructor parameter name; internal so only the label
    // creates one, and there is no path to a version without its root.
    internal GlobalLabelVersion(
        GlobalLabelVersionId id,
        int versionNumber)
    {
        Id = id;
        VersionNumber = versionNumber;
        Status = GlobalLabelVersionStatus.Draft;
    }

    /// <summary>Assigned by the label. Never accepted from a caller.</summary>
    public int VersionNumber { get; private set; }

    public GlobalLabelVersionStatus Status { get; private set; }

    /// <summary>
    /// The file this version is. Nullable while a draft is being prepared, and
    /// required before it can be published.
    /// </summary>
    public ProductDocumentId? ContentId { get; private set; }

    /// <summary>What changed from the previous version, in the author's words.</summary>
    public string? ChangeSummary { get; private set; }

    /// <summary>
    /// The business date this version takes effect — supplied, never read from
    /// the clock, so a label family migrated from a shared drive can state when
    /// each issue actually applied.
    /// </summary>
    public DateOnly? EffectiveFrom { get; private set; }

    /// <summary>
    /// The last day this version was in force. Written by the label when a later
    /// version supersedes it, never supplied: the two dates must meet exactly,
    /// and a caller allowed to set this could leave a gap or an overlap.
    /// </summary>
    public DateOnly? EffectiveTo { get; private set; }

    /// <summary>
    /// When the publish happened, as against when it took effect. A version
    /// approved in March to apply from June has two different dates, and losing
    /// either one loses a question someone asks.
    /// </summary>
    public DateTime? PublishedOnUtc { get; private set; }

    /// <summary>True on the one version a reader means by "the current label".</summary>
    public bool IsInForce => Status == GlobalLabelVersionStatus.InForce;

    internal void AttachContent(ProductDocumentId contentId)
    {
        RequireDraft();

        ContentId = contentId;
    }

    internal void Summarise(string? changeSummary)
    {
        RequireDraft();

        if (changeSummary is not null
            && changeSummary.Trim().Length > ChangeSummaryMaxLength)
        {
            throw new DomainException(GlobalLabelErrors.ChangeSummaryTooLong);
        }

        ChangeSummary = string.IsNullOrWhiteSpace(changeSummary)
            ? null
            : changeSummary.Trim();
    }

    internal void Publish(DateOnly effectiveFrom, DateTime publishedOnUtc)
    {
        RequireDraft();

        if (effectiveFrom == default)
            throw new DomainException(GlobalLabelErrors.EffectiveFromRequired);

        // The rule that makes the content link load-bearing rather than
        // decorative: a version with no document is a number, and a number is
        // not a label.
        if (ContentId is null)
            throw new BusinessRuleViolationException(
                GlobalLabelErrors.ContentRequiredToPublish);

        Status = GlobalLabelVersionStatus.InForce;
        EffectiveFrom = effectiveFrom;
        PublishedOnUtc = publishedOnUtc;
    }

    /// <param name="lastDayInForce">
    /// The day before its replacement takes effect, computed by the label.
    /// Inclusive, so a version in force for a single day has
    /// <see cref="EffectiveFrom"/> equal to <see cref="EffectiveTo"/>.
    /// </param>
    internal void Supersede(DateOnly lastDayInForce)
    {
        Status = GlobalLabelVersionStatus.Superseded;
        EffectiveTo = lastDayInForce;
    }

    private void RequireDraft()
    {
        if (Status != GlobalLabelVersionStatus.Draft)
            throw new BusinessRuleViolationException(
                GlobalLabelErrors.VersionNotDraft);
    }
}
