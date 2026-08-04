using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.LocalLabels;

public sealed class LocalLabelRevisionId : StronglyTypedId
{
    public LocalLabelRevisionId(Guid value) : base(value)
    {
    }

    public static LocalLabelRevisionId New() => new(Guid.NewGuid());

    public static LocalLabelRevisionId From(Guid value) => new(value);

    public static implicit operator Guid(LocalLabelRevisionId id) => id.Value;
}
