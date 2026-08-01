using RegOS.SharedKernel.Primitives;

namespace RegOS.Interaction.Domain.Meetings;

public sealed class HaMeetingId : StronglyTypedId
{
    public HaMeetingId(Guid value) : base(value)
    {
    }

    public static HaMeetingId New() => new(Guid.NewGuid());

    public static HaMeetingId From(Guid value) => new(value);

    public static implicit operator Guid(HaMeetingId id) => id.Value;
}
