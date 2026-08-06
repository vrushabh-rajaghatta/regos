using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Aggregates.ProcessPlans;

public sealed class ProcessStepId : StronglyTypedId
{
    public ProcessStepId(Guid value) : base(value)
    {
    }

    public static ProcessStepId New() => new(Guid.NewGuid());

    public static ProcessStepId From(Guid value) => new(value);

    public static implicit operator Guid(ProcessStepId id) => id.Value;
}
