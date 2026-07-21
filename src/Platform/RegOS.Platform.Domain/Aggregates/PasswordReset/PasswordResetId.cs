using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Domain.Aggregates.PasswordReset;

public sealed class PasswordResetId : StronglyTypedId
{
    public PasswordResetId(Guid value) : base(value)
    {
    }

    public static PasswordResetId New() => new(Guid.NewGuid());

    public static PasswordResetId From(Guid value) => new(value);

    public static implicit operator Guid(PasswordResetId id) => id.Value;
}
