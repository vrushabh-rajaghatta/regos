using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Domain.Aggregates.User;

public sealed class UserId : StronglyTypedId
{
    public UserId(Guid value) : base(value)
    {
    }

    public static UserId New() => new(Guid.NewGuid());

    public static UserId From(Guid value) => new(value);

    public static implicit operator Guid(UserId id) => id.Value;
}
