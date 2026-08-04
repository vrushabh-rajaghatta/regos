namespace RegOS.Product.Domain.Product;

public static class PharmaceuticalProductDetailErrors
{
    public const string TenantRequired =
        "A presentation must belong to a tenant.";

    public const string MarketRequired =
        "A presentation must belong to a market.";

    public const string NameRequired =
        "A presentation name is required.";

    public static readonly string NameTooLong =
        $"A presentation name must be "
        + $"{PharmaceuticalProductDetail.NameMaxLength} characters or fewer.";

    public static readonly string DescriptionTooLong =
        $"A description must be "
        + $"{PharmaceuticalProductDetail.DescriptionMaxLength} characters or fewer.";

    public const string DoseFormRequired =
        "A presentation must have a dose form.";

    /// <remarks>
    /// The one rule worth stating rather than tolerating. A route repeated on
    /// one presentation is not extra information — it is the same fact twice,
    /// and it would be rendered twice in every downstream listing.
    /// </remarks>
    public const string RouteAlreadyRecorded =
        "This presentation already lists that route of administration.";

    public const string NotFound =
        "Presentation does not exist.";
}
