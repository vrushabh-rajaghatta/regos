using RegOS.SharedKernel.Primitives;

namespace RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

public sealed class ApplicationStudyCitationId : StronglyTypedId
{
    public ApplicationStudyCitationId(Guid value) : base(value)
    {
    }

    public static ApplicationStudyCitationId New() => new(Guid.NewGuid());

    public static ApplicationStudyCitationId From(Guid value) => new(value);

    public static implicit operator Guid(ApplicationStudyCitationId id)
        => id.Value;
}
