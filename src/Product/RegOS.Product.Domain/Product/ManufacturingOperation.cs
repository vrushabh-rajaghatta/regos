using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// A site performing one operation for one market's product, over a period —
/// <em>"Site Gamma has released batches for the German product since March
/// 2024."</em>
/// </summary>
/// <remarks>
/// <b>The single place that says where work happens</b>
/// (<see href="../../../docs/adr/ADR-063-where-a-product-is-made-is-a-product-fact.md">ADR-063</see>
/// §3). RIM puts a <c>Manufacturer</c> on <c>Packaging</c> and another on
/// <c>Packaged Product</c>; RegOS keeps neither, because the distinction those
/// columns were drawing — who packs it, who tests it, who releases it — is
/// carried by <see cref="Operation"/> on this one relationship. Three columns
/// saying the same thing in three places is three ways for them to disagree.
/// <para>
/// <b>It lives in Product, and D2 is why.</b> <c>Ingredient</c> cannot leave
/// this context and needs a site of its own, so <c>Product.Domain</c> →
/// <c>Organization.Domain</c> exists whatever happens here; once it does,
/// hosting this in Organization would need the reverse edge and close a cycle.
/// Worth contrasting with
/// <see href="../../../docs/adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md">ADR-061</see>
/// §3, where the reverse edge already existed and <b>the compiler refused the
/// design</b>. Here it does not refuse, so the argument is written down instead.
/// </para>
/// <para>
/// <b>Market-local, like everything else in this tier</b>
/// (<see href="../../../docs/adr/ADR-039-the-market-local-product-tier.md">ADR-039</see>):
/// secondary packaging in particular is done per market, and the question this
/// answers — <em>is this site on <b>this</b> licence?</em> — compares against
/// one market's authorisation.
/// </para>
/// <para>
/// <b>What it does not record.</b> Not the process, not its steps, not the
/// materials each step consumes — those are CMC narrative and the dossier
/// already carries them at 3.2.S.2 and 3.2.P.3.3. Not whether the site is
/// <em>allowed</em> to perform the operation: that is a licence's statement
/// (S002) and a quality system's (EPIC-008), and keeping them apart is what
/// lets RegOS report a divergence between them at all.
/// </para>
/// </remarks>
public sealed class ManufacturingOperation
    : AggregateRoot<ManufacturingOperationId>
{
    private ManufacturingOperation()
    {
    }

    /// <summary>The owning tenant (ADR-031). Fail-closed, set once.</summary>
    public TenantId TenantId { get; private set; } = default!;

    /// <summary>The market whose product is made — not the global product.</summary>
    public MedicinalProductId MedicinalProductId { get; private set; } = default!;

    /// <summary>Where the work happens.</summary>
    public OrganizationSiteId OrganizationSiteId { get; private set; } = default!;

    /// <summary>
    /// What the site does, from
    /// <see cref="ManufacturingVocabulary.Operations"/>.
    /// </summary>
    /// <remarks>
    /// <b>A coded value, not an enum, and nothing branches on it.</b> The moment
    /// a rule reads this field's code to decide something, it has stopped being
    /// vocabulary — which is the test <c>OrganizationSiteType</c>'s docstring
    /// records for going the other way (EPIC-010c D4).
    /// </remarks>
    public CodedConcept Operation { get; private set; } = default!;

    /// <summary>
    /// The business date the site started performing it.
    /// </summary>
    /// <remarks>
    /// Supplied rather than read from the clock, so an operation recorded today
    /// can say it has run since 2019 — the same call <c>OrganizationSite</c>
    /// makes about <c>StatusDate</c>.
    /// </remarks>
    public DateOnly EffectiveFrom { get; private set; }

    /// <summary>
    /// The date it stopped, or null while it is still running.
    /// </summary>
    /// <remarks>
    /// <b>A period, not a status history</b> (EPIC-010c D5). <em>"This site
    /// performs this operation between these dates"</em> is a dated fact, not a
    /// lifecycle a regulator took positions in — so it earns no
    /// <c>StatusEntry</c> child, and the
    /// <see href="../../../docs/product/BACKLOG.md">status-history rule</see>
    /// exempts it explicitly.
    /// <para>
    /// <b>A transfer closes one period and opens another</b> rather than editing
    /// a site id in place, which is what keeps <em>"who released our batches in
    /// 2023?"</em> answerable.
    /// </para>
    /// </remarks>
    public DateOnly? CeasedOn { get; private set; }

    /// <summary>True while the site still performs this operation.</summary>
    public bool IsCurrent => CeasedOn is null;

    public DateTime RecordedOnUtc { get; private set; }

    public static ManufacturingOperation Record(
        TenantId tenantId,
        MedicinalProductId medicinalProductId,
        OrganizationSiteId organizationSiteId,
        CodedConcept operation,
        DateOnly effectiveFrom)
    {
        if (tenantId is null)
            throw new DomainException(ManufacturingOperationErrors.TenantRequired);

        if (medicinalProductId is null)
            throw new DomainException(ManufacturingOperationErrors.MarketRequired);

        if (organizationSiteId is null)
            throw new DomainException(ManufacturingOperationErrors.SiteRequired);

        if (operation is null)
            throw new DomainException(
                ManufacturingOperationErrors.OperationRequired);

        if (effectiveFrom == default)
            throw new DomainException(
                ManufacturingOperationErrors.EffectiveFromRequired);

        return new ManufacturingOperation
        {
            Id = ManufacturingOperationId.New(),
            TenantId = tenantId,
            MedicinalProductId = medicinalProductId,
            OrganizationSiteId = organizationSiteId,
            Operation = operation,
            EffectiveFrom = effectiveFrom,
            RecordedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Closes the period — the site no longer performs this operation.
    /// </summary>
    /// <remarks>
    /// <b>Closed, never deleted</b> (ES-018). A site that made this product for
    /// four years made it; removing the row would make a filing from 2023
    /// unexplainable. A transfer is this call followed by a new operation, and
    /// the pair reads as the history it is.
    /// </remarks>
    public void Cease(DateOnly ceasedOn)
    {
        if (CeasedOn is not null)
            throw new BusinessRuleViolationException(
                ManufacturingOperationErrors.AlreadyCeased);

        if (ceasedOn == default)
            throw new DomainException(
                ManufacturingOperationErrors.EffectiveFromRequired);

        if (ceasedOn < EffectiveFrom)
            throw new BusinessRuleViolationException(
                ManufacturingOperationErrors.CeasedBeforeItStarted);

        CeasedOn = ceasedOn;
    }

    /// <summary>
    /// Corrects the dates. The market, the site and the operation are immutable.
    /// </summary>
    /// <remarks>
    /// A different site or a different operation is a <em>different</em>
    /// operation, and editing one into another would leave no way to tell a
    /// correction from a transfer — the same call <c>PackAuthorisation.Correct</c>
    /// makes about the pair it names.
    /// </remarks>
    public void Correct(DateOnly effectiveFrom, DateOnly? ceasedOn)
    {
        if (effectiveFrom == default)
            throw new DomainException(
                ManufacturingOperationErrors.EffectiveFromRequired);

        if (ceasedOn is { } ceased && ceased < effectiveFrom)
            throw new BusinessRuleViolationException(
                ManufacturingOperationErrors.CeasedBeforeItStarted);

        EffectiveFrom = effectiveFrom;
        CeasedOn = ceasedOn;
    }
}
