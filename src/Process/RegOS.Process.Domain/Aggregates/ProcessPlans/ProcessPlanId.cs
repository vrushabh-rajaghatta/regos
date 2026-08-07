using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Aggregates.ProcessPlans;

public sealed class ProcessPlanId : StronglyTypedId
{
    public ProcessPlanId(Guid value) : base(value)
    {
    }

    public static ProcessPlanId New() => new(Guid.NewGuid());

    public static ProcessPlanId From(Guid value) => new(value);

    public static implicit operator Guid(ProcessPlanId id) => id.Value;
}
