namespace RegOS.ReferenceData.Domain.Terminology;

public static class PharmaceuticalVocabularyErrors
{
    public static string UnknownDoseForm(string? code)
        => $"\"{code}\" is not a dose form RegOS knows. "
            + $"Accepted: {Codes(PharmaceuticalVocabulary.DoseForms)}.";

    public static string UnknownRoute(string? code)
        => $"\"{code}\" is not a route of administration RegOS knows. "
            + $"Accepted: {Codes(PharmaceuticalVocabulary.RoutesOfAdministration)}.";

    public static string UnknownUnitOfPresentation(string? code)
        => $"\"{code}\" is not a unit of presentation RegOS knows. "
            + $"Accepted: {Codes(PharmaceuticalVocabulary.UnitsOfPresentation)}.";

    private static string Codes(IReadOnlyList<CodedConcept> vocabulary)
        => string.Join(", ", vocabulary.Select(x => x.Code));
}
