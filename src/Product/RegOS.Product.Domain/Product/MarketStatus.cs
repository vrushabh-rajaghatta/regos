namespace RegOS.Product.Domain.Product;

/// <summary>
/// What is commercially true of a product in one market — as distinct from
/// what the regulator has done, which is <c>RegistrationStatus</c>.
/// </summary>
/// <remarks>
/// The two are independent facts. A product can be <c>Approved</c> and never
/// launched; launched and then temporarily unavailable while the licence stands
/// untouched; discontinued commercially while the authorisation is still valid.
/// <para>
/// <b>There is deliberately no <c>Withdrawn</c> here.</b> That word already
/// means a surrendered licence at <c>RegistrationStatus.Withdrawn</c>, and the
/// portfolio views show both at once. The rule is not "never reuse a word
/// across tiers" — <see cref="Planned"/> appears at both and means the same
/// thing at each. It is <b>never let one word carry two meanings</b>, and a
/// surrendered licence is not the same event as ceasing to sell.
/// </para>
/// </remarks>
public enum MarketStatus
{
    /// <summary>
    /// We intend to market here. The state every market presence begins in,
    /// and the only one that cannot be returned to: a market already entered
    /// cannot be planned again.
    /// </summary>
    Planned = 0,

    /// <summary>On sale.</summary>
    Launched = 1,

    /// <summary>
    /// Off sale and expected back — a supply interruption, a manufacturing
    /// transfer. The licence is untouched.
    /// </summary>
    TemporarilyUnavailable = 2,

    /// <summary>
    /// Ceased, not expected to return. Not terminal in the model: a product
    /// genuinely can be relaunched years later, and forbidding it would encode
    /// one company's commercial history as universal law.
    /// </summary>
    Discontinued = 3,
}
