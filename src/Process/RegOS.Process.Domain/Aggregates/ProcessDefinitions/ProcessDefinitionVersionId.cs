using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Aggregates.ProcessDefinitions;

public sealed class ProcessDefinitionVersionId : StronglyTypedId
{
    public ProcessDefinitionVersionId(Guid value) : base(value)
    {
    }

    public static ProcessDefinitionVersionId New() => new(Guid.NewGuid());

    public static ProcessDefinitionVersionId From(Guid value) => new(value);

    public static implicit operator Guid(ProcessDefinitionVersionId id) => id.Value;
}
