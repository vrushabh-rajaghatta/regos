namespace RegOS.Submission.Domain.Submission;

public readonly record struct SubmissionId(Guid Value)
{
    public static SubmissionId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SubmissionId id)
        => id.Value;
}
