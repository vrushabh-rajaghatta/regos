namespace RegOS.Submission.Domain.Snapshot;

public readonly record struct SubmissionSnapshotId(Guid Value)
{
    public static SubmissionSnapshotId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SubmissionSnapshotId id)
        => id.Value;
}
