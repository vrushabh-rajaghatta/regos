namespace RegOS.Product.Domain.Product;

/// <summary>
/// Whether this market-local <em>record</em> is in use — an activation flag, so
/// it carries a single <c>StatusDate</c> rather than a history.
/// </summary>
/// <remarks>
/// <b>Not market status, not registration status, and not deletion.</b> Three
/// separate questions are asked of a market presence, and this answers only the
/// last of them:
/// <list type="bullet">
/// <item><c>RegistrationStatus</c> — what has the regulator done?</item>
/// <item><see cref="MarketStatus"/> — is the product on sale?</item>
/// <item>this — should this record participate in normal work?</item>
/// </list>
/// A product can be launched, temporarily unavailable, then discontinued while
/// its record stays <see cref="Active"/> throughout; and a record can be
/// <see cref="Inactive"/> while the market it describes is still selling under
/// a valid licence.
/// </remarks>
public enum MedicinalProductStatus
{
    /// <summary>
    /// This market record participates in normal operational workflows.
    /// </summary>
    Active = 0,

    /// <summary>
    /// This market record is retained for history but intentionally excluded
    /// from operational workflows. <b>Deactivation implies no regulatory or
    /// commercial state</b> — it does not withdraw a licence, does not take a
    /// product off sale, and does not delete anything (ES-018).
    /// </summary>
    Inactive = 1,
}
