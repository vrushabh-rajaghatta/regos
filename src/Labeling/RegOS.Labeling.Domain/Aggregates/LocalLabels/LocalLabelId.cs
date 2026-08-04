using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.LocalLabels;

public sealed class LocalLabelId : StronglyTypedId
{
    public LocalLabelId(Guid value) : base(value)
    {
    }

    public static LocalLabelId New() => new(Guid.NewGuid());

    public static LocalLabelId From(Guid value) => new(value);

    public static implicit operator Guid(LocalLabelId id) => id.Value;
}
