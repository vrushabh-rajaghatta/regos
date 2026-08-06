using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Aggregates.ProcessObjectives;

public sealed class ProcessObjectiveId : StronglyTypedId
{
    public ProcessObjectiveId(Guid value) : base(value)
    {
    }

    public static ProcessObjectiveId New() => new(Guid.NewGuid());

    public static ProcessObjectiveId From(Guid value) => new(value);

    public static implicit operator Guid(ProcessObjectiveId id) => id.Value;
}
