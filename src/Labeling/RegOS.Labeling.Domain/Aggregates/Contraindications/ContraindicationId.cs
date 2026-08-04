using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.Contraindications;

public sealed class ContraindicationId : StronglyTypedId
{
    public ContraindicationId(Guid value) : base(value)
    {
    }

    public static ContraindicationId New() => new(Guid.NewGuid());

    public static ContraindicationId From(Guid value) => new(value);

    public static implicit operator Guid(ContraindicationId id) => id.Value;
}
