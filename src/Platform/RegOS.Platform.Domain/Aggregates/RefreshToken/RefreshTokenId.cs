using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Domain.Aggregates.RefreshToken;

public sealed class RefreshTokenId : StronglyTypedId
{
    public RefreshTokenId(Guid value) : base(value)
    {
    }

    public static RefreshTokenId New() => new(Guid.NewGuid());

    public static RefreshTokenId From(Guid value) => new(value);

    public static implicit operator Guid(RefreshTokenId id) => id.Value;
}
