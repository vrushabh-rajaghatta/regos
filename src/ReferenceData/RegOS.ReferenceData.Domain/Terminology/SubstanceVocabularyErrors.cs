namespace RegOS.ReferenceData.Domain.Terminology;

public static class SubstanceVocabularyErrors
{
    /// <remarks>
    /// Lists the accepted words rather than saying "invalid". A caller who sent
    /// the wrong code cannot guess the right one, and EPIC-019 settled that a
    /// refusal names what it would have accepted.
    /// </remarks>
    public static string UnknownClass(string? code)
        => $"\"{code}\" is not a substance class RegOS knows. "
            + $"Accepted: {Codes(SubstanceVocabulary.Classes)}.";

    public static string UnknownType(string? code)
        => $"\"{code}\" is not a substance type RegOS knows. "
            + $"Accepted: {Codes(SubstanceVocabulary.Types)}.";

    private static string Codes(IReadOnlyList<CodedConcept> vocabulary)
        => string.Join(", ", vocabulary.Select(x => x.Code));
}
