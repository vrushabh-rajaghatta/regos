namespace RegOS.Submission.Domain.Submission;

public enum SubmissionStatus
{
    Draft = 1,

    // A published dossier is frozen — its document set can no longer change.
    // Transmission to the authority is a separate, later step.
    Published = 2
}
