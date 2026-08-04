using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.GlobalLabels;

public sealed class GlobalLabelVersionId : StronglyTypedId
{
    public GlobalLabelVersionId(Guid value) : base(value)
    {
    }

    public static GlobalLabelVersionId New() => new(Guid.NewGuid());

    public static GlobalLabelVersionId From(Guid value) => new(value);

    public static implicit operator Guid(GlobalLabelVersionId id) => id.Value;
}
