using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Domain.Aggregates.Session;

public sealed class SessionId : StronglyTypedId
{
    public SessionId(Guid value) : base(value)
    {
    }

    public static SessionId New() => new(Guid.NewGuid());

    public static SessionId From(Guid value) => new(value);

    public static implicit operator Guid(SessionId id) => id.Value;
}
