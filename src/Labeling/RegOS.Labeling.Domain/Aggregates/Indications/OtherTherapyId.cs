using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.Indications;

public sealed class OtherTherapyId : StronglyTypedId
{
    public OtherTherapyId(Guid value) : base(value)
    {
    }

    public static OtherTherapyId New() => new(Guid.NewGuid());

    public static OtherTherapyId From(Guid value) => new(value);

    public static implicit operator Guid(OtherTherapyId id) => id.Value;
}
