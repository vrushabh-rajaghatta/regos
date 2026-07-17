namespace RegOS.Submission.Domain.Submission;

public enum SubmissionStatus
{
    Draft = 1,

    // A submitted dossier is frozen — its document set can no longer change.
    Submitted = 2
}
