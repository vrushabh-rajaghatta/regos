using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Aggregates.ProcessPlans;

public sealed class ProcessStepStatusEntryId : StronglyTypedId
{
    public ProcessStepStatusEntryId(Guid value) : base(value)
    {
    }

    public static ProcessStepStatusEntryId New() => new(Guid.NewGuid());

    public static ProcessStepStatusEntryId From(Guid value) => new(value);

    public static implicit operator Guid(ProcessStepStatusEntryId id) => id.Value;
}
