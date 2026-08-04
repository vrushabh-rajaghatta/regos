namespace RegOS.ReferenceData.Domain.Terminology;

public static class SupplyVocabularyErrors
{
    public static string UnknownLegalStatus(string? code)
        => $"\"{code}\" is not a legal status of supply RegOS knows. "
            + $"Accepted: {Codes(SupplyVocabulary.LegalStatuses)}.";

    public static string UnknownStorageCondition(string? code)
        => $"\"{code}\" is not a storage condition RegOS knows. "
            + $"Accepted: {Codes(SupplyVocabulary.StorageConditions)}.";

    public static string UnknownShelfLifePeriod(string? code)
        => $"\"{code}\" is not a shelf-life period RegOS knows. "
            + $"Accepted: {Codes(SupplyVocabulary.ShelfLifePeriods)}.";

    private static string Codes(IReadOnlyList<CodedConcept> vocabulary)
        => string.Join(", ", vocabulary.Select(x => x.Code));
}
