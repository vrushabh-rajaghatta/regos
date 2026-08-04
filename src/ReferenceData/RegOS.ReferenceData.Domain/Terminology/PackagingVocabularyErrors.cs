namespace RegOS.ReferenceData.Domain.Terminology;

public static class PackagingVocabularyErrors
{
    public static string UnknownPackageItemType(string? code)
        => $"\"{code}\" is not a pack layer RegOS knows. "
            + $"Accepted: {Codes(PackagingVocabulary.PackageItemTypes)}.";

    public static string UnknownMaterial(string? code)
        => $"\"{code}\" is not a packaging material RegOS knows. "
            + $"Accepted: {Codes(PackagingVocabulary.Materials)}.";

    private static string Codes(IReadOnlyList<CodedConcept> vocabulary)
        => string.Join(", ", vocabulary.Select(x => x.Code));
}
