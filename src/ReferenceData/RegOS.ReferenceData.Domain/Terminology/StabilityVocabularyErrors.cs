namespace RegOS.ReferenceData.Domain.Terminology;

public static class StabilityVocabularyErrors
{
    public static string UnknownCondition(string? code)
        => $"\"{code}\" is not a stability testing condition RegOS knows. "
            + $"Accepted: {Codes(StabilityVocabulary.Conditions)}.";

    private static string Codes(IReadOnlyList<CodedConcept> vocabulary)
        => string.Join(", ", vocabulary.Select(x => x.Code));
}
