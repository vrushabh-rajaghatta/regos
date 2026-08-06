using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Aggregates.ProcessObjectives;

public sealed class ProcessObjectiveStatusEntryId : StronglyTypedId
{
    public ProcessObjectiveStatusEntryId(Guid value) : base(value)
    {
    }

    public static ProcessObjectiveStatusEntryId New() => new(Guid.NewGuid());

    public static ProcessObjectiveStatusEntryId From(Guid value) => new(value);

    public static implicit operator Guid(ProcessObjectiveStatusEntryId id) => id.Value;
}
