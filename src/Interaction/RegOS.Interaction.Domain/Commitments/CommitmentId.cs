using RegOS.SharedKernel.Primitives;

namespace RegOS.Interaction.Domain.Commitments;

public sealed class CommitmentId : StronglyTypedId
{
    public CommitmentId(Guid value) : base(value)
    {
    }

    public static CommitmentId New() => new(Guid.NewGuid());

    public static CommitmentId From(Guid value) => new(value);

    public static implicit operator Guid(CommitmentId id) => id.Value;
}
