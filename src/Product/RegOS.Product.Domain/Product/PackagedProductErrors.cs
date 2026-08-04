namespace RegOS.Product.Domain.Product;

public static class PackagedProductErrors
{
    public const string NotFound =
        "That pack does not exist.";

    public const string TenantRequired =
        "A pack must belong to a tenant.";

    public const string MedicinalProductRequired =
        "A pack must belong to a market.";

    public const string DescriptionRequired =
        "Describe the pack — what a person would read off the carton.";

    public const string DescriptionTooLong =
        "The pack description is too long.";

    public const string PackCodeTooLong =
        "The pack code is too long.";

    /// <remarks>
    /// Names the ambiguity rather than the field, the way the population age
    /// guards do: <em>30</em> with no unit could be tablets, millilitres or
    /// vials, and a unit with no quantity says nothing at all.
    /// </remarks>
    public const string PackSizeUnitRequired =
        "A pack size needs a unit — 30 could be tablets, millilitres or vials.";

    public const string PackSizeQuantityRequired =
        "A pack size unit needs a quantity.";

    public const string PackSizeMustBePositive =
        "A pack size must be greater than zero.";

    public const string UnitOfPresentationNotRecognised =
        "That unit of presentation is not in the vocabulary.";

    public const string MarketingStatusNotRecognised =
        "That marketing status is not recognised.";

    public const string OccurredOnRequired =
        "Say when this became true for the pack.";

    public const string OccurredOnBeforePreviousEntry =
        "A pack's status cannot take effect before the one it replaces.";

    public const string PackCannotBePlannedAgain =
        "A pack that has reached the market cannot be planned again.";

    public const string NoteTooLong =
        "The note is too long.";

    /// <remarks>
    /// A pack always carries a shelf-life statement, and
    /// <c>ShelfLifeStorage.NotStated</c> is the empty one — so null is a caller
    /// mistake rather than a way to clear it.
    /// </remarks>
    public const string ShelfLifeRequired =
        "A pack always has a shelf-life statement, even an empty one.";

    public static string AlreadyInMarketingStatus(PackageMarketingStatus status)
        => $"This pack is already {status}.";
}
