using RegOS.SharedKernel.Primitives;

namespace RegOS.Submission.Domain.Submission;

public sealed class SubmissionStatusEntryId : StronglyTypedId
{
    public SubmissionStatusEntryId(Guid value) : base(value)
    {
    }

    public static SubmissionStatusEntryId New() => new(Guid.NewGuid());

    public static SubmissionStatusEntryId From(Guid value) => new(value);

    public static implicit operator Guid(SubmissionStatusEntryId id) => id.Value;
}
