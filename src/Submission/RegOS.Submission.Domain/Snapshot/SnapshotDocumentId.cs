namespace RegOS.Submission.Domain.Snapshot;

public readonly record struct SnapshotDocumentId(Guid Value)
{
    public static SnapshotDocumentId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SnapshotDocumentId id)
        => id.Value;
}
