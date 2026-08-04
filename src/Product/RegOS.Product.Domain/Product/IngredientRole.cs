namespace RegOS.Product.Domain.Product;

/// <summary>
/// What a substance is doing in a formulation.
/// </summary>
/// <remarks>
/// <b>An enum, and not a <c>CodedConcept</c> — a departure from EPIC-010a's
/// entity table, taken on a stated test.</b> The test is: <em>does a rule
/// branch on this value?</em>
/// <list type="bullet">
/// <item>Nothing branches on dose form, route, substance class or unit — they
/// are described, rendered and filtered, never reasoned about. Those are
/// terminology.</item>
/// <item>Two rules branch on this one: a composition must contain at least one
/// <see cref="Active"/>, and an active must declare a strength. A coded concept
/// whose code a rule string-matches is an enum wearing a costume, and the
/// costume hides that changing the vocabulary would change behaviour.</item>
/// </list>
/// <para>
/// <b>Revisit when a role arrives that no rule branches on</b> — adjuvant,
/// stabiliser, ISO 11238's longer list. At that point the roles split into the
/// ones the model reasons about and the ones it merely records, and that is a
/// different modelling problem from this one.
/// </para>
/// </remarks>
public enum IngredientRole
{
    /// <summary>The substance the product works by. Must declare a strength.</summary>
    Active = 1,

    /// <summary>
    /// Everything else in the formulation. Its quantity is often genuinely not
    /// declared — <em>q.s.</em> — so a strength is optional.
    /// </summary>
    Excipient = 2
}
