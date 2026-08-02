using RegOS.SharedKernel.Primitives;

namespace RegOS.Submission.Domain.Submission;

public sealed class SubmissionId : StronglyTypedId
{
    public SubmissionId(Guid value) : base(value)
    {
    }

    public static SubmissionId New() => new(Guid.NewGuid());

    public static SubmissionId From(Guid value) => new(value);

    public static implicit operator Guid(SubmissionId id) => id.Value;
}
