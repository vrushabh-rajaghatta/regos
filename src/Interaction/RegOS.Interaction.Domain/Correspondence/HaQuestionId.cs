using RegOS.SharedKernel.Primitives;

namespace RegOS.Interaction.Domain.Correspondence;

public sealed class HaQuestionId : StronglyTypedId
{
    public HaQuestionId(Guid value) : base(value)
    {
    }

    public static HaQuestionId New() => new(Guid.NewGuid());

    public static HaQuestionId From(Guid value) => new(value);

    public static implicit operator Guid(HaQuestionId id) => id.Value;
}
