namespace RegOS.ReferenceData.Domain.Terminology;

public static class PharmaceuticalVocabularyErrors
{
    public static string UnknownDoseForm(string? code)
        => $"\"{code}\" is not a dose form RegOS knows. "
            + $"Accepted: {Codes(PharmaceuticalVocabulary.DoseForms)}.";

    public static string UnknownRoute(string? code)
        => $"\"{code}\" is not a route of administration RegOS knows. "
            + $"Accepted: {Codes(PharmaceuticalVocabulary.RoutesOfAdministration)}.";

    public static string UnknownComponentType(string? code)
        => $"\"{code}\" is not a component type RegOS knows. "
            + $"Accepted: {Codes(PharmaceuticalVocabulary.ComponentTypes)}.";

    public static string UnknownUnitOfPresentation(string? code)
        => $"\"{code}\" is not a unit of presentation RegOS knows. "
            + $"Accepted: {Codes(PharmaceuticalVocabulary.UnitsOfPresentation)}.";

    public static string UnknownColour(string? code)
        => $"\"{code}\" is not a colour RegOS knows. "
            + $"Accepted: {Codes(PharmaceuticalVocabulary.Colours)}.";

    public static string UnknownShape(string? code)
        => $"\"{code}\" is not a shape RegOS knows. "
            + $"Accepted: {Codes(PharmaceuticalVocabulary.Shapes)}.";

    private static string Codes(IReadOnlyList<CodedConcept> vocabulary)
        => string.Join(", ", vocabulary.Select(x => x.Code));
}
