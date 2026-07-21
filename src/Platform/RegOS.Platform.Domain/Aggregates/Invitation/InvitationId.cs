using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Domain.Aggregates.Invitation;

public sealed class InvitationId : StronglyTypedId
{
    public InvitationId(Guid value) : base(value)
    {
    }

    public static InvitationId New() => new(Guid.NewGuid());

    public static InvitationId From(Guid value) => new(value);

    public static implicit operator Guid(InvitationId id) => id.Value;
}
