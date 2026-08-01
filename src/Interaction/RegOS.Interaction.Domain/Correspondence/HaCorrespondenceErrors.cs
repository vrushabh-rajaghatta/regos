namespace RegOS.Interaction.Domain.Correspondence;

public static class HaCorrespondenceErrors
{
    public const string TenantRequired =
        "A tenant is required.";

    public const string AuthorityRequired =
        "The health authority is required.";

    public const string CorrespondenceTypeRequired =
        "The correspondence type is required.";

    public const string SubjectRequired =
        "A subject is required.";

    public static readonly string SubjectTooLong =
        $"A subject cannot exceed {HaCorrespondence.SubjectMaxLength} characters.";

    public static readonly string ReferenceTooLong =
        $"An authority reference cannot exceed {HaCorrespondence.ReferenceMaxLength} characters.";

    // The one chronology rule. Who owes the response is derived from the
    // direction, not enforced here: an inbound letter is one we must answer,
    // and an outbound one may still carry a date the authority has committed to
    // (a meeting-request clock). Both are facts about the same letter.
    public const string ResponseDueBeforeOccurred =
        "A response cannot be due before the correspondence itself.";
}
