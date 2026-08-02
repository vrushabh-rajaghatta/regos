using RegOS.SharedKernel.Primitives;

namespace RegOS.Submission.Domain.Submission;

public sealed class SubmissionRoleId : StronglyTypedId
{
    public SubmissionRoleId(Guid value) : base(value)
    {
    }

    public static SubmissionRoleId New() => new(Guid.NewGuid());

    public static SubmissionRoleId From(Guid value) => new(value);

    public static implicit operator Guid(SubmissionRoleId id) => id.Value;
}
