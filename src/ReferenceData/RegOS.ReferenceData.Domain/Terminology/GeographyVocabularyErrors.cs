namespace RegOS.ReferenceData.Domain.Terminology;

public static class GeographyVocabularyErrors
{
    public static string UnknownRegion(string? code)
        => $"\"{code}\" is not a regulatory grouping RegOS knows. "
            + $"Accepted: {Codes(GeographyVocabulary.Regions)}.";

    private static string Codes(IReadOnlyList<CodedConcept> vocabulary)
        => string.Join(", ", vocabulary.Select(x => x.Code));
}
