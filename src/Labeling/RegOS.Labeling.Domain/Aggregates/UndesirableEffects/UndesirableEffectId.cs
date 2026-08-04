using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.UndesirableEffects;

public sealed class UndesirableEffectId : StronglyTypedId
{
    public UndesirableEffectId(Guid value) : base(value)
    {
    }

    public static UndesirableEffectId New() => new(Guid.NewGuid());

    public static UndesirableEffectId From(Guid value) => new(value);

    public static implicit operator Guid(UndesirableEffectId id) => id.Value;
}
