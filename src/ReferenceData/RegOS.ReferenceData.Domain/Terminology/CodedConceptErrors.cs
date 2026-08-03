namespace RegOS.ReferenceData.Domain.Terminology;

public static class CodedConceptErrors
{
    public const string SystemRequired =
        "A coded concept must say which vocabulary it comes from.";

    public static readonly string SystemTooLong =
        $"A coding system cannot exceed {CodedConcept.SystemMaxLength} characters.";

    public const string CodeRequired =
        "A coded concept must have a code.";

    public static readonly string CodeTooLong =
        $"A code cannot exceed {CodedConcept.CodeMaxLength} characters.";

    public const string DisplayRequired =
        "A coded concept must have a display name.";

    public static readonly string DisplayTooLong =
        $"A display name cannot exceed {CodedConcept.DisplayMaxLength} characters.";
}
