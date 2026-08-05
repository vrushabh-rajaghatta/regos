namespace RegOS.Product.Domain.Product;

public static class ShelfLifeStorageErrors
{
    /// <remarks>
    /// Names the ambiguity rather than the field, the way the pack-size guards
    /// do: <em>36</em> alone could be days, months or years.
    /// </remarks>
    public const string PeriodUnitRequired =
        "A shelf life needs a period — 36 could be days, months or years.";

    public const string PeriodValueRequired =
        "A shelf-life period needs a number.";

    public const string PeriodMustBePositive =
        "A shelf life must be greater than zero.";

    public const string TextTooLong =
        "The shelf-life wording is too long.";

    public const string ConditionAlreadyStated =
        "That storage condition is already on this pack.";

    /// <remarks>
    /// Worded so it cannot be mistaken for <see cref="ConditionAlreadyStated"/>
    /// above: one is about how the pack must be kept, the other about what its
    /// shelf life was demonstrated under.
    /// </remarks>
    public const string TestedAtAlreadyStated =
        "That stability testing condition is already on this pack.";

    /// <remarks>
    /// The invariant that keeps <em>"none required"</em> a conclusion rather
    /// than a blank: a pack that needs no special precautions cannot also name
    /// one.
    /// </remarks>
    public const string NoSpecialPrecautionsStandsAlone =
        "\"No special storage precautions\" cannot sit beside a precaution — "
        + "remove one or the other.";
}
