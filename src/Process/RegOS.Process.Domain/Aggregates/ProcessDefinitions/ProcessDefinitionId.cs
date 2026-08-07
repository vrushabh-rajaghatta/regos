using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Aggregates.ProcessDefinitions;

public sealed class ProcessDefinitionId : StronglyTypedId
{
    public ProcessDefinitionId(Guid value) : base(value)
    {
    }

    public static ProcessDefinitionId New() => new(Guid.NewGuid());

    public static ProcessDefinitionId From(Guid value) => new(value);

    public static implicit operator Guid(ProcessDefinitionId id) => id.Value;
}
