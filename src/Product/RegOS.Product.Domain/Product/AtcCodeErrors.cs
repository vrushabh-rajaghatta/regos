namespace RegOS.Product.Domain.Product;

public static class AtcCodeErrors
{
    public const string Required =
        "An ATC code is required.";

    /// <remarks>
    /// Shows the shape rather than naming a regular expression, and says
    /// plainly that RegOS is not checking the code exists — the user should not
    /// read acceptance here as verification.
    /// </remarks>
    public const string Malformed =
        "An ATC code looks like N02BE01 — a letter, two digits, two letters, "
        + "two digits, and may stop at any level (N, N02, N02B, N02BE). "
        + "RegOS checks the shape only; it does not hold the WHO ATC index.";
}
