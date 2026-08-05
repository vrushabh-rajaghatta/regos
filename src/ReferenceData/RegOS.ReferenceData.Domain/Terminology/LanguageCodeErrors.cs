namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// The refusals <see cref="LanguageCode"/> makes.
/// </summary>
/// <remarks>
/// <b>Moved out of <c>MedicinalProductErrors</c> with the type</b> (ADR-062).
/// A language code borrowing a medicinal product's error vocabulary is the same
/// misplacement in miniature that the move corrects — and it was invisible
/// until the type crossed the boundary and stopped compiling.
/// </remarks>
public static class LanguageCodeErrors
{
    public const string Required =
        "A language is required.";

    public const string NotRecognised =
        "A language is a two-letter ISO 639-1 code, such as en or fr.";
}
