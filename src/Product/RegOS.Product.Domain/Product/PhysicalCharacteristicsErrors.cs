namespace RegOS.Product.Domain.Product;

public static class PhysicalCharacteristicsErrors
{
    public const string ImprintTooLong =
        "The marking is too long — record what is stamped on it, not how it "
        + "looks.";

    public const string DescriptionTooLong =
        "The appearance wording is too long.";

    public const string ColourAlreadyStated =
        "That colour is already on this presentation.";

    /// <remarks>
    /// A presentation always carries an appearance, and
    /// <c>PhysicalCharacteristics.NotStated</c> is the empty one — so null is a
    /// caller mistake rather than a way to clear it.
    /// </remarks>
    public const string AppearanceRequired =
        "A presentation always has an appearance, even an empty one.";
}
