using RegOS.SharedKernel.Primitives;

namespace RegOS.Submission.Domain.Snapshot;

public sealed class SubmissionSnapshotId : StronglyTypedId
{
    public SubmissionSnapshotId(Guid value) : base(value)
    {
    }

    public static SubmissionSnapshotId New() => new(Guid.NewGuid());

    public static SubmissionSnapshotId From(Guid value) => new(value);

    public static implicit operator Guid(SubmissionSnapshotId id) => id.Value;
}
