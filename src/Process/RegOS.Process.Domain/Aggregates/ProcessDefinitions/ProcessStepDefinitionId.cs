using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Aggregates.ProcessDefinitions;

public sealed class ProcessStepDefinitionId : StronglyTypedId
{
    public ProcessStepDefinitionId(Guid value) : base(value)
    {
    }

    public static ProcessStepDefinitionId New() => new(Guid.NewGuid());

    public static ProcessStepDefinitionId From(Guid value) => new(value);

    public static implicit operator Guid(ProcessStepDefinitionId id) => id.Value;
}
