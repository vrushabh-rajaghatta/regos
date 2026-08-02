using RegOS.SharedKernel.Primitives;

namespace RegOS.Submission.Domain.Submission;

public sealed class SubmissionDocumentId : StronglyTypedId
{
    public SubmissionDocumentId(Guid value) : base(value)
    {
    }

    public static SubmissionDocumentId New() => new(Guid.NewGuid());

    public static SubmissionDocumentId From(Guid value) => new(value);

    public static implicit operator Guid(SubmissionDocumentId id) => id.Value;
}
