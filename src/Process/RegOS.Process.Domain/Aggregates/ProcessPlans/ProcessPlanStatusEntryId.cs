using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Aggregates.ProcessPlans;

public sealed class ProcessPlanStatusEntryId : StronglyTypedId
{
    public ProcessPlanStatusEntryId(Guid value) : base(value)
    {
    }

    public static ProcessPlanStatusEntryId New() => new(Guid.NewGuid());

    public static ProcessPlanStatusEntryId From(Guid value) => new(value);

    public static implicit operator Guid(ProcessPlanStatusEntryId id) => id.Value;
}
