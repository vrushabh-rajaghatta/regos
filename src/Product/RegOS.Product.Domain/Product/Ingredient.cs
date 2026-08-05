using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Substances;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// The role a substance plays in one presentation, at one strength.
/// </summary>
/// <remarks>
/// <b>This is the row that makes the epic's question answerable.</b>
/// <see cref="SubstanceId"/> points at a shared fact rather than repeating a
/// name, so <em>"which of our products contain substance X?"</em> can be asked
/// backwards — from the substance to the products — instead of matching on
/// strings (ADR-058).
/// <para>
/// <b>A child entity of <see cref="PharmaceuticalProductDetail"/>, not a
/// root.</b> Nobody quotes its id, no query reaches it directly, and it has no
/// lifecycle of its own: composition is stated and restated as a whole. It
/// therefore carries no <c>TenantId</c> — it is reachable only through a
/// filtered root (ADR-031), the same shape <c>TradeName</c> takes one tier up.
/// </para>
/// <para>
/// <b>One parent, so there is no polymorphism to solve.</b> RIM allows an
/// ingredient beneath a component as well; nothing in RegOS demonstrates that,
/// and Q3 asks what the patient physically receives rather than what a
/// component is made of. One demonstrated need is not two (EPIC-010a D3).
/// </para>
/// <para>
/// <b>The seam this type recorded in EPIC-010a was taken in EPIC-010c S003</b>,
/// and the paragraph is kept because it named its own trigger:
/// <blockquote><em>"No manufacturing source. 'Which products use a substance
/// sourced from site Y?' would justify a nullable organisation id here; nobody
/// has asked it, sourcing belongs to cluster D, and the answer today would be an
/// empty column. Recorded as a seam, not built."</em></blockquote>
/// **Cluster D is EPIC-010c, and it asked.** A prediction that names what would
/// change the answer is worth more than a count of occurrences — the lesson
/// EPIC-022 recorded, firing for the third time in this epic.
/// </para>
/// </remarks>
public sealed class Ingredient : Entity<IngredientId>
{
    // For EF only. Unlike TradeName beside it, this entity cannot be
    // materialised through its real constructor: Strength is an owned type, and
    // owned types cannot bind to constructor parameters. The validating
    // constructor below stays the only way application code can make one.
    private Ingredient()
    {
    }

    // Only the presentation creates these.
    internal Ingredient(
        IngredientId id,
        SubstanceId substanceId,
        IngredientRole role,
        Strength? strength,
        OrganizationSiteId? manufacturingSourceSiteId = null)
    {
        if (substanceId is null)
            throw new DomainException(IngredientErrors.SubstanceRequired);

        // The rule that makes the role load-bearing rather than descriptive: a
        // product works by its actives, and an active nobody has quantified is
        // an incomplete formulation rather than a formulation with a blank.
        // An excipient's quantity is routinely not declared — "q.s." — so its
        // absence is a fact rather than a gap.
        if (role == IngredientRole.Active && strength is null)
            throw new DomainException(IngredientErrors.ActiveNeedsAStrength);

        Id = id;
        SubstanceId = substanceId;
        Role = role;
        Strength = strength;
        ManufacturingSourceSiteId = manufacturingSourceSiteId;
    }

    /// <summary>The shared fact this ingredient is an instance of.</summary>
    public SubstanceId SubstanceId { get; private set; } = default!;

    public IngredientRole Role { get; private set; }

    /// <summary>
    /// Required for an <see cref="IngredientRole.Active"/>, optional for an
    /// excipient.
    /// </summary>
    public Strength? Strength { get; private set; }

    /// <summary>
    /// <b>Where does this substance come from?</b> — the site that makes it.
    /// </summary>
    /// <remarks>
    /// <b>Not <c>ManufacturingOperation</c> restated, and the two model
    /// different stages of the supply chain</b>
    /// (<see href="../../../docs/adr/ADR-063-where-a-product-is-made-is-a-product-fact.md">ADR-063</see>
    /// §2). That one answers <em>which sites perform an operation for this
    /// product</em>; this answers <em>where this active substance
    /// originated</em>, and they diverge in cases that are ordinary rather than
    /// exotic:
    /// <code>
    /// Finished product          made at Site Gamma      ← ManufacturingOperation
    /// ├── API A                 from Site Alpha         ← this field
    /// └── API B                 from Site Beta          ← this field
    /// </code>
    /// <b>Neither is derivable from the other.</b> An operation set cannot say
    /// which API came from where; a source cannot say who packed the carton. A
    /// single "manufacturer" field would answer neither and look like it
    /// answered both.
    /// <para>
    /// <b>Nullable, and it will stay mostly null — which is the honest
    /// state.</b> RegOS holds no provenance for any ingredient recorded before
    /// this shipped, and a column that claimed otherwise would be worse than an
    /// empty one. Absent means <em>nobody has said</em>, never <em>unsourced</em>.
    /// </para>
    /// <para>
    /// <b>An id, not a name.</b> The site is joined on read like every other
    /// site reference in this epic; there is no manufacturer name stored
    /// anywhere in RegOS (ADR-063 §3).
    /// </para>
    /// </remarks>
    public OrganizationSiteId? ManufacturingSourceSiteId { get; private set; }
}
