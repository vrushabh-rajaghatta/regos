namespace RegOS.Submission.Domain.Submission;

public readonly record struct SubmissionDocumentId(Guid Value)
{
    public static SubmissionDocumentId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SubmissionDocumentId id)
        => id.Value;
}
