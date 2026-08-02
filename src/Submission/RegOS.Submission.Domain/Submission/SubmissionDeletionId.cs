using RegOS.SharedKernel.Primitives;

namespace RegOS.Submission.Domain.Submission;

public sealed class SubmissionDeletionId : StronglyTypedId
{
    public SubmissionDeletionId(Guid value) : base(value)
    {
    }

    public static SubmissionDeletionId New() => new(Guid.NewGuid());

    public static SubmissionDeletionId From(Guid value) => new(value);

    public static implicit operator Guid(SubmissionDeletionId id) => id.Value;
}
