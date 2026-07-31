namespace RegOS.Product.Domain.Product;

/// <summary>
/// Whether this market-local record is in use — an activation flag, so it
/// carries a single <c>StatusDate</c> rather than a history.
/// </summary>
/// <remarks>
/// <b>Not market status.</b> "Is this record live in RegOS" and "is the product
/// actually on sale in this country" are different questions with different
/// lifecycles: a product can be launched, temporarily unavailable, then
/// discontinued while its record stays active throughout. Market status is a
/// dated business history and arrives in EPIC-017 S003 as its own concept.
/// </remarks>
public enum MedicinalProductStatus
{
    Active = 0,
    Inactive = 1,
}
