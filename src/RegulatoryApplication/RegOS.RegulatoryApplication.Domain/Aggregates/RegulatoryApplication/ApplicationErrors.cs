namespace RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

/// <summary>
/// Domain error messages for the Application lifecycle. Kept generic so they
/// can be reused across transition methods.
/// </summary>
public static class ApplicationErrors
{
    public const string ApplicationDoesNotExist =
        "That application does not exist.";

    /// <remarks>
    /// <b>Correctable until it has been filed, and never after.</b>
    /// <c>us-regional.xml</c> renders the application number into every
    /// published sequence, so changing it once a sequence has carried it to the
    /// authority would rewrite what was filed — the reasoning ADR-045 and
    /// ADR-047 apply to everything else frozen at publication.
    /// </remarks>
    public const string ApplicationNumberIsFiled =
        "This application has filed sequence {0} under the number {1}, so that "
        + "number is what the authority received and cannot be changed here. "
        + "If the authority reassigned it, that is a new regulatory event.";

    public const string InvalidStatusTransition =
        "The requested status transition is not allowed.";

    public const string ApplicationAlreadyActive =
        "The application is already active.";

    public const string ApplicationAlreadyClosed =
        "The application is closed and can no longer change status.";

    // Study citations (EPIC-019 S004)
    public const string StudyIsNotCited =
        "This application does not cite that study.";
}
