using RegOS.SharedKernel.Primitives;

namespace RegOS.Interaction.Domain.Correspondence;

public sealed class CorrespondenceAttachmentId : StronglyTypedId
{
    public CorrespondenceAttachmentId(Guid value) : base(value)
    {
    }

    public static CorrespondenceAttachmentId New() => new(Guid.NewGuid());

    public static CorrespondenceAttachmentId From(Guid value) => new(value);

    public static implicit operator Guid(CorrespondenceAttachmentId id) => id.Value;
}
