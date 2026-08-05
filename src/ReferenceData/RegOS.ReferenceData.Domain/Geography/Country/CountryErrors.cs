namespace RegOS.ReferenceData.Domain.Geography.Country;

public static class CountryErrors
{
    public const string CodeRequired =
        "Country code is required.";

    public const string NameRequired =
        "Country name is required.";

    public const string IsoAlpha3CodeRequired =
        "An ISO 3166-1 alpha-3 code is required — it is what machine-readable "
        + "submissions name the country by.";

    /// <remarks>
    /// The two code columns are one keystroke apart, and an alpha-2 value here
    /// would be carried into every downstream message without anything noticing.
    /// </remarks>
    public const string IsoAlpha3CodeMalformed =
        "An ISO 3166-1 alpha-3 code is exactly three letters — USA, IND, JPN.";

    public const string IsoNameRequired =
        "The official ISO country name is required.";

    public const string IsoNameTooLong =
        "The official ISO country name is too long.";
}
