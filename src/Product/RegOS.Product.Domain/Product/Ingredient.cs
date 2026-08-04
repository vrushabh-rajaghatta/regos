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
/// <b>No manufacturing source.</b> <em>"Which products use a substance sourced
/// from site Y?"</em> would justify a nullable organisation id here; nobody has
/// asked it, sourcing belongs to cluster D, and the answer today would be an
/// empty column. Recorded as a seam, not built.
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
        Strength? strength)
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
    }

    /// <summary>The shared fact this ingredient is an instance of.</summary>
    public SubstanceId SubstanceId { get; private set; } = default!;

    public IngredientRole Role { get; private set; }

    /// <summary>
    /// Required for an <see cref="IngredientRole.Active"/>, optional for an
    /// excipient.
    /// </summary>
    public Strength? Strength { get; private set; }
}
