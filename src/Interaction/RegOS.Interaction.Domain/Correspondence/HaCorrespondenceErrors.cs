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

    public const string FileNameRequired =
        "A file name is required.";

    public static readonly string FileNameTooLong =
        $"A file name cannot exceed {CorrespondenceAttachment.FileNameMaxLength} characters.";

    public const string ContentTypeRequired =
        "A content type is required.";

    public const string StoragePathRequired =
        "A storage path is required.";

    public const string FileEmpty =
        "An empty file cannot be attached.";

    public const string AttachmentNotFound =
        "That attachment is not on this correspondence.";

    public const string QuestionNumberRequired =
        "A question number is required.";

    public static readonly string QuestionNumberTooLong =
        $"A question number cannot exceed {HaQuestion.NumberMaxLength} characters.";

    public const string QuestionTextRequired =
        "The question text is required.";

    public static readonly string QuestionTextTooLong =
        $"A question cannot exceed {HaQuestion.TextMaxLength} characters.";

    public const string ResponseRequired =
        "A response is required.";

    public static readonly string ResponseTooLong =
        $"A response cannot exceed {HaQuestion.ResponseMaxLength} characters.";

    public const string QuestionAlreadyResolved =
        "That question is already resolved.";

    public const string QuestionNotFound =
        "That question is not on this correspondence.";

    public const string QuestionNumberNotUnique =
        "That question number is already used on this correspondence.";

    public const string QuestionHistoryOutOfOrder =
        "A question's history cannot go backwards in time.";

    public static readonly string NoteTooLong =
        $"A note cannot exceed {HaQuestionStatusEntry.NoteMaxLength} characters.";

    // The one chronology rule. Who owes the response is derived from the
    // direction, not enforced here: an inbound letter is one we must answer,
    // and an outbound one may still carry a date the authority has committed to
    // (a meeting-request clock). Both are facts about the same letter.
    public const string ResponseDueBeforeOccurred =
        "A response cannot be due before the correspondence itself.";
}
