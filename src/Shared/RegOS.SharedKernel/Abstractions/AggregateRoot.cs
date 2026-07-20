using RegOS.SharedKernel.Primitives;

namespace RegOS.SharedKernel.Abstractions;

/// <summary>
/// Marks an entity as the root of an aggregate: the transactional consistency
/// boundary that owns its child entities and is the only member of the aggregate
/// a repository loads or saves.
/// </summary>
/// <remarks>
/// Intentionally empty for now — its value here is semantic, not functional. When
/// a real need appears, this is where aggregate-wide concerns (domain events,
/// concurrency/versioning, integration events) will live. Nothing is added
/// speculatively.
/// </remarks>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : StronglyTypedId
{
}
