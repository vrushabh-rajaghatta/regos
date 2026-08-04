using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.Indications;

public sealed class PopulationId : StronglyTypedId
{
    public PopulationId(Guid value) : base(value)
    {
    }

    public static PopulationId New() => new(Guid.NewGuid());

    public static PopulationId From(Guid value) => new(value);

    public static implicit operator Guid(PopulationId id) => id.Value;
}
