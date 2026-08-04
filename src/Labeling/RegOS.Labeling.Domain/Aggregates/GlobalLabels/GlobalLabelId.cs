using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.GlobalLabels;

public sealed class GlobalLabelId : StronglyTypedId
{
    public GlobalLabelId(Guid value) : base(value)
    {
    }

    public static GlobalLabelId New() => new(Guid.NewGuid());

    public static GlobalLabelId From(Guid value) => new(value);

    public static implicit operator Guid(GlobalLabelId id) => id.Value;
}
