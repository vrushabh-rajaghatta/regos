namespace RegOS.Product.Domain.Product;

/// <summary>
/// What is commercially true of <em>one pack</em>, as distinct from the market
/// it is sold in.
/// </summary>
/// <remarks>
/// <b>The same four words as <see cref="MarketStatus"/>, and deliberately its
/// own enum.</b> They are independent facts: a market can be launched while its
/// 100-tablet pack is discontinued and its 30 is on sale. Merging the two would
/// let a rule added to one silently reach the other — the same call EPIC-018
/// made when it refused to share one status enum between global and local label
/// revisions.
/// <para>
/// <b>No transition table</b>, for the reason <see cref="MarketStatus"/> gives:
/// commercial reality is not a constrained graph. A pack may be withdrawn from
/// sale and reintroduced, and encoding one company's history as universal law is
/// what <c>RegistrationLifecycle</c>'s own governing principle forbids.
/// </para>
/// </remarks>
public enum PackageMarketingStatus
{
    /// <summary>
    /// We intend to supply this pack. The state every pack begins in, and the
    /// only one that cannot be returned to — a pack that has reached the market
    /// cannot be intended again.
    /// </summary>
    Planned = 0,

    /// <summary>On sale.</summary>
    Marketed = 1,

    /// <summary>
    /// Off sale and expected back — a supply interruption, a printing
    /// changeover. The licence and the pack itself are untouched.
    /// </summary>
    TemporarilyUnavailable = 2,

    /// <summary>
    /// Ceased, not expected to return. Not terminal: a pack size genuinely can
    /// be reintroduced years later.
    /// </summary>
    Discontinued = 3,
}
