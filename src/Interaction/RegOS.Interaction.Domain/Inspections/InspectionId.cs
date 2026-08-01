using RegOS.SharedKernel.Primitives;

namespace RegOS.Interaction.Domain.Inspections;

public sealed class InspectionId : StronglyTypedId
{
    public InspectionId(Guid value) : base(value)
    {
    }

    public static InspectionId New() => new(Guid.NewGuid());

    public static InspectionId From(Guid value) => new(value);

    public static implicit operator Guid(InspectionId id) => id.Value;
}
