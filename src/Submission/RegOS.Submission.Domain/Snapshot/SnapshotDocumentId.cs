using RegOS.SharedKernel.Primitives;

namespace RegOS.Submission.Domain.Snapshot;

public sealed class SnapshotDocumentId : StronglyTypedId
{
    public SnapshotDocumentId(Guid value) : base(value)
    {
    }

    public static SnapshotDocumentId New() => new(Guid.NewGuid());

    public static SnapshotDocumentId From(Guid value) => new(value);

    public static implicit operator Guid(SnapshotDocumentId id) => id.Value;
}
