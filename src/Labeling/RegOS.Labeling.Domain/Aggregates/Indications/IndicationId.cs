using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.Indications;

public sealed class IndicationId : StronglyTypedId
{
    public IndicationId(Guid value) : base(value)
    {
    }

    public static IndicationId New() => new(Guid.NewGuid());

    public static IndicationId From(Guid value) => new(value);

    public static implicit operator Guid(IndicationId id) => id.Value;
}
