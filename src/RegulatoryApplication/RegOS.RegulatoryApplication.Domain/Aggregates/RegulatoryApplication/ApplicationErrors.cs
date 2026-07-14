namespace RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

/// <summary>
/// Domain error messages for the Application lifecycle. Kept generic so they
/// can be reused across transition methods.
/// </summary>
public static class ApplicationErrors
{
    public const string InvalidStatusTransition =
        "The requested status transition is not allowed.";

    public const string ApplicationAlreadyActive =
        "The application is already active.";

    public const string ApplicationAlreadyClosed =
        "The application is closed and can no longer change status.";
}
