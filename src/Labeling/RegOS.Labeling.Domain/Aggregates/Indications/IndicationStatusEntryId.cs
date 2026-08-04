using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.Indications;

public sealed class IndicationStatusEntryId : StronglyTypedId
{
    public IndicationStatusEntryId(Guid value) : base(value)
    {
    }

    public static IndicationStatusEntryId New() => new(Guid.NewGuid());

    public static IndicationStatusEntryId From(Guid value) => new(value);

    public static implicit operator Guid(IndicationStatusEntryId id) => id.Value;
}
