using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.DrugInteractions;

public sealed class DrugInteractionId : StronglyTypedId
{
    public DrugInteractionId(Guid value) : base(value)
    {
    }

    public static DrugInteractionId New() => new(Guid.NewGuid());

    public static DrugInteractionId From(Guid value) => new(value);

    public static implicit operator Guid(DrugInteractionId id) => id.Value;
}
