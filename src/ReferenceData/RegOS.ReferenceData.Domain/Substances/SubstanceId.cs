using RegOS.SharedKernel.Primitives;

namespace RegOS.ReferenceData.Domain.Substances;

public sealed class SubstanceId : StronglyTypedId
{
    public SubstanceId(Guid value) : base(value)
    {
    }

    public static SubstanceId New() => new(Guid.NewGuid());

    public static SubstanceId From(Guid value) => new(value);

    public static implicit operator Guid(SubstanceId id) => id.Value;
}
