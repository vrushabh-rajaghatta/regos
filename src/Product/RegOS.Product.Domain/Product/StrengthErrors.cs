namespace RegOS.Product.Domain.Product;

public static class StrengthErrors
{
    public const string NumeratorMustBePositive =
        "A strength must be greater than zero.";

    public const string NumeratorUnitRequired =
        "A strength must have a unit.";

    public const string DenominatorMustBePositive =
        "A strength's denominator must be greater than zero.";

    /// <remarks>
    /// Names both halves, because the caller has one of them and needs to know
    /// which is missing.
    /// </remarks>
    public const string DenominatorUnitRequired =
        "A strength expressed per a quantity needs a unit for that quantity — "
        + "10 mg per 1 mL, not 10 mg per 1.";

    public const string DenominatorValueRequired =
        "A strength expressed per a unit needs a quantity for it — "
        + "10 mg per 1 mL, not 10 mg per mL.";
}
