namespace RegOS.ReferenceData.Domain.Terminology;

public static class ManufacturingVocabularyErrors
{
    public static string UnknownOperation(string? code)
        => $"\"{code}\" is not a manufacturing operation RegOS knows. "
            + $"Accepted: {Codes(ManufacturingVocabulary.Operations)}.";

    private static string Codes(IReadOnlyList<CodedConcept> vocabulary)
        => string.Join(", ", vocabulary.Select(x => x.Code));
}
