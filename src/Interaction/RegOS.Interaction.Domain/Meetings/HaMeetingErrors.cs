namespace RegOS.Interaction.Domain.Meetings;

public static class HaMeetingErrors
{
    public const string TenantRequired = "A tenant is required.";

    public const string AuthorityRequired = "A meeting is with an authority.";

    public const string SubjectRequired = "A subject is required.";

    public static readonly string SubjectTooLong =
        $"A subject cannot exceed {HaMeeting.SubjectMaxLength} characters.";

    public const string InvalidInitialStatus =
        "A meeting begins either Requested (we asked) or Granted (they called it).";

    public static readonly string TransitionNotAllowed =
        "A meeting cannot move to that status from where it is.";

    public const string AlreadyConcluded =
        "A meeting that was held, declined or cancelled cannot change status.";

    public const string HistoryOutOfOrder =
        "A meeting's history cannot go backwards in time.";

    public const string OutcomeBeforeHeld =
        "A meeting's outcome can only be recorded once it has been held.";

    public static readonly string MinutesTooLong =
        $"Minutes cannot exceed {HaMeeting.MinutesMaxLength} characters.";

    public static readonly string OutcomeTooLong =
        $"An outcome cannot exceed {HaMeeting.OutcomeMaxLength} characters.";

    public static readonly string NoteTooLong =
        $"A note cannot exceed {HaMeetingStatusEntry.NoteMaxLength} characters.";
}
