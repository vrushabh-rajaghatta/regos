using RegOS.SharedKernel.Primitives;

namespace RegOS.Registration.Domain.Aggregates.PackAuthorisations;

public sealed class PackAuthorisationId : StronglyTypedId
{
    public PackAuthorisationId(Guid value) : base(value)
    {
    }

    public static PackAuthorisationId New() => new(Guid.NewGuid());

    public static PackAuthorisationId From(Guid value) => new(value);

    public static implicit operator Guid(PackAuthorisationId id) => id.Value;
}
