using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.DrugInteractions;

public sealed class InteractantId : StronglyTypedId
{
    public InteractantId(Guid value) : base(value)
    {
    }

    public static InteractantId New() => new(Guid.NewGuid());

    public static InteractantId From(Guid value) => new(value);

    public static implicit operator Guid(InteractantId id) => id.Value;
}
