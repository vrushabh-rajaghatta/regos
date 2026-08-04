namespace RegOS.Product.Application.Services;

/// <summary>
/// Rules the Product context enforces about facts owned elsewhere, so they
/// cannot live in either domain.
/// </summary>
public static class ProductRuleErrors
{
    /// <remarks>
    /// The same message whether the substance never existed or belongs to
    /// another tenant. A distinguishing message would confirm the existence of
    /// a row the caller may not see.
    /// </remarks>
    public const string SubstanceDoesNotExist =
        "Substance does not exist.";
}
