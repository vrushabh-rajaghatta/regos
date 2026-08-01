using RegOS.SharedKernel.Primitives;

namespace RegOS.Interaction.Domain.Correspondence;

public sealed class HaCorrespondenceId : StronglyTypedId
{
    public HaCorrespondenceId(Guid value) : base(value)
    {
    }

    public static HaCorrespondenceId New() => new(Guid.NewGuid());

    public static HaCorrespondenceId From(Guid value) => new(value);

    public static implicit operator Guid(HaCorrespondenceId id) => id.Value;
}
