using RegOS.SharedKernel.Primitives;

namespace RegOS.Registration.Domain.Aggregates.SiteApprovals;

public sealed class SiteApprovalId : StronglyTypedId
{
    public SiteApprovalId(Guid value) : base(value)
    {
    }

    public static SiteApprovalId New() => new(Guid.NewGuid());

    public static SiteApprovalId From(Guid value) => new(value);

    public static implicit operator Guid(SiteApprovalId id) => id.Value;
}
