namespace RegOS.Interaction.Domain.Inspections;

public static class InspectionErrors
{
    public const string TenantRequired = "A tenant is required.";

    public const string AuthorityRequired = "An inspection is by an authority.";

    public const string TitleRequired = "A title is required.";

    public static readonly string TitleTooLong =
        $"A title cannot exceed {Inspection.TitleMaxLength} characters.";

    public const string InvalidInitialStatus =
        "An inspection begins either Announced (they told us) or InProgress (they arrived).";

    public const string AlreadyConcluded =
        "A completed or cancelled inspection cannot change status.";

    public const string CannotReturnToAnnounced =
        "An inspection that has started cannot become announced.";

    public const string AlreadyInThatStatus =
        "The inspection already holds that status.";

    public const string HistoryOutOfOrder =
        "An inspection's history cannot go backwards in time.";

    public const string OutcomeBeforeCompleted =
        "Findings can only be recorded once the inspection has completed.";

    public static readonly string OutcomeTooLong =
        $"Findings cannot exceed {Inspection.OutcomeMaxLength} characters.";

    public static readonly string NoteTooLong =
        $"A note cannot exceed {InspectionStatusEntry.NoteMaxLength} characters.";
}
