using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.GlobalLabels;

/// <summary>
/// A label a company holds centrally, above any market — the core data sheet
/// and its siblings, versioned, with one version in force at a time.
/// </summary>
/// <remarks>
/// <b>An editorial artifact, not a regulatory fact</b>
/// ([ADR-059](../../../../../docs/adr/ADR-059-clinical-statements-are-facts-labels-are-artifacts.md)).
/// It publishes what the company has decided to say; what a product is
/// <em>approved</em> to say is an <c>Indication</c> or a <c>Contraindication</c>
/// on a market-local product, and lives on its own clock. Nothing here points at
/// those, deliberately — see ADR-059 §3 for the five questions that link would
/// have to answer first.
/// <para>
/// It hangs off <see cref="GlobalProductId"/> because a core data sheet is held
/// for the molecule as the company sells it worldwide. The market's own label is
/// <c>LocalLabel</c>, which arrives in S002 and derives from a version of this.
/// </para>
/// <para>
/// <b>Nothing enforces one label per product per type.</b> A company may hold
/// two patient leaflets for one product where the audiences differ, and
/// uniqueness we cannot justify is uniqueness that will be wrong for somebody —
/// the same call <c>MedicinalProduct</c> made on <c>(GlobalProductId,
/// CountryId)</c>.
/// </para>
/// </remarks>
public sealed class GlobalLabel : AggregateRoot<GlobalLabelId>
{
    public const int NameMaxLength = 200;

    private readonly List<GlobalLabelVersion> _versions = [];

    // Parameterless, unlike GlobalProduct beside it — and the difference is
    // LabelType. EF binds constructor parameters by name from mapped
    // properties, and an owned type is not one of those: a value object cannot
    // be bound to a parameter at all. So this aggregate is materialised
    // property-by-property and Create uses an object initializer, exactly as
    // PharmaceuticalProductDetail does for the same reason.
    private GlobalLabel()
    {
    }

    /// <summary>The owning tenant (ADR-031). Set once.</summary>
    public TenantId TenantId { get; private set; } = default!;

    /// <summary>
    /// The product this label is held for. Immutable — repointing it would
    /// silently rewrite what every version underneath describes.
    /// </summary>
    public GlobalProductId GlobalProductId { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    /// <summary>
    /// What kind of label this is — core data sheet, core safety information,
    /// patient leaflet. Terminology rather than a domain type: nothing branches
    /// on it, and every kind versions identically (ADR-059 §7).
    /// </summary>
    public CodedConcept LabelType { get; private set; } = default!;

    public DateTime CreatedOnUtc { get; private set; }

    /// <summary>
    /// Every issue of this label, oldest first once ordered. A label always has
    /// at least one — <see cref="Create"/> opens the first draft, because a
    /// label with no version is a name with nothing behind it.
    /// </summary>
    public IReadOnlyCollection<GlobalLabelVersion> Versions
        => _versions.AsReadOnly();

    /// <summary>
    /// The one version a reader means by "the current label", or null while the
    /// first draft is still being written.
    /// </summary>
    public GlobalLabelVersion? VersionInForce
        => _versions.FirstOrDefault(x => x.IsInForce);

    /// <summary>The open draft, if one is being prepared. At most one.</summary>
    public GlobalLabelVersion? Draft
        => _versions.FirstOrDefault(
            x => x.Status == GlobalLabelVersionStatus.Draft);

    public static GlobalLabel Create(
        TenantId tenantId,
        GlobalProductId globalProductId,
        string? name,
        string? labelTypeCode,
        DateTime createdOnUtc)
    {
        if (tenantId is null)
            throw new DomainException(GlobalLabelErrors.TenantRequired);

        if (globalProductId is null)
            throw new DomainException(GlobalLabelErrors.GlobalProductRequired);

        var label = new GlobalLabel
        {
            Id = GlobalLabelId.New(),
            TenantId = tenantId,
            GlobalProductId = globalProductId,
            Name = Normalize(name),
            LabelType = ResolveType(labelTypeCode),
            CreatedOnUtc = createdOnUtc
        };

        // A label exists to hold versions, so the first draft is opened with it
        // rather than as a second call the caller could forget. The same shape
        // MedicinalProduct uses for its first market-status entry.
        label.StartDraft();

        return label;
    }

    /// <summary>
    /// Opens the next draft (N+1). The aggregate owns numbering — a version
    /// number is never accepted from outside — and permits one open draft.
    /// </summary>
    public GlobalLabelVersion StartDraft()
    {
        if (Draft is not null)
            throw new BusinessRuleViolationException(
                GlobalLabelErrors.DraftAlreadyOpen);

        // Max, not Count + 1 and not [^1]. Nothing orders the collection the
        // database hands back, and a version could in principle be removed;
        // deriving the next number from the highest one that exists is the only
        // form that cannot renumber an issue somebody has already cited.
        var nextNumber = _versions.Count == 0
            ? 1
            : _versions.Max(x => x.VersionNumber) + 1;

        var version = new GlobalLabelVersion(
            GlobalLabelVersionId.New(),
            nextNumber);

        _versions.Add(version);

        return version;
    }

    /// <summary>
    /// Points this label's open draft at the file it is. The document belongs to
    /// <c>ProductDocument</c> and keeps its own lifecycle; this records only
    /// that it is what this version says (ADR-059 §6).
    /// </summary>
    public void AttachContent(
        GlobalLabelVersionId versionId,
        ProductDocumentId contentId)
        => VersionOf(versionId).AttachContent(contentId);

    public void SummariseChanges(
        GlobalLabelVersionId versionId,
        string? changeSummary)
        => VersionOf(versionId).Summarise(changeSummary);

    /// <summary>
    /// Puts a draft in force from a date, and retires the version it replaces.
    /// </summary>
    /// <remarks>
    /// <b>The two acts are one method because they are one fact.</b> A label
    /// family with two versions in force is not a state a company can be in, and
    /// a caller able to publish without superseding — or to supersede without a
    /// replacement — could produce one. The supersede date is computed here
    /// rather than supplied for the same reason: the ranges must meet exactly,
    /// with no gap and no overlap.
    /// </remarks>
    public void PublishVersion(
        GlobalLabelVersionId versionId,
        DateOnly effectiveFrom,
        DateTime publishedOnUtc)
    {
        var version = VersionOf(versionId);
        var current = VersionInForce;

        // Business time moves forward, and the boundary is what makes "which
        // version applied on date X" answerable. Same day is refused too: two
        // versions both in force on one date is exactly the ambiguity the rule
        // exists to prevent.
        if (current is not null
            && current.EffectiveFrom is { } from
            && effectiveFrom <= from)
        {
            throw new DomainException(
                GlobalLabelErrors.EffectiveFromNotAfterVersionInForce);
        }

        version.Publish(effectiveFrom, publishedOnUtc);

        current?.Supersede(effectiveFrom.AddDays(-1));
    }

    /// <summary>
    /// Throws away the open draft. There is no draft afterwards, so the next
    /// <see cref="StartDraft"/> reopens the same number.
    /// </summary>
    /// <remarks>
    /// <b>The one deletion in this aggregate, and it does not contradict
    /// ES-018.</b> Lifecycle-over-deletion protects records that were once true;
    /// a draft has never been in force, was never cited, and never described
    /// what the company said about a product. Discarding one loses no regulatory
    /// fact — and without it, a draft started by mistake is permanent: the
    /// "one open draft" rule blocks a replacement, and publishing needs content
    /// nobody intends to attach.
    /// <para>
    /// A version that has ever been in force cannot reach here — the guard is
    /// <see cref="GlobalLabelVersionStatus.Draft"/>, not "not in force", so a
    /// superseded issue is as untouchable as the current one.
    /// </para>
    /// </remarks>
    public void DiscardDraft()
    {
        var draft = Draft
            ?? throw new NotFoundException(GlobalLabelErrors.NoOpenDraft);

        _versions.Remove(draft);
    }

    private GlobalLabelVersion VersionOf(GlobalLabelVersionId versionId)
        => _versions.FirstOrDefault(x => x.Id == versionId)
           ?? throw new NotFoundException(GlobalLabelErrors.VersionNotFound);

    private static string Normalize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(GlobalLabelErrors.NameRequired);

        var trimmed = name.Trim();

        return trimmed.Length > NameMaxLength
            ? throw new DomainException(GlobalLabelErrors.NameTooLong)
            : trimmed;
    }

    /// <remarks>
    /// The vocabulary hands back a fresh instance per call, and that is not a
    /// detail: an owned coded value is tracked against exactly one owner, and
    /// sharing one instance across two labels persists nulls (ADR-059 §7, the
    /// defect EPIC-010a S001 paid for).
    /// </remarks>
    private static CodedConcept ResolveType(string? labelTypeCode)
    {
        if (string.IsNullOrWhiteSpace(labelTypeCode))
            throw new DomainException(GlobalLabelErrors.LabelTypeRequired);

        return LabelVocabulary.GlobalLabelTypeOf(labelTypeCode)
               ?? throw new DomainException(
                   GlobalLabelErrors.LabelTypeNotRecognised);
    }
}
