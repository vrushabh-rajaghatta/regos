using RegOS.SharedKernel.Primitives;

namespace RegOS.SharedKernel.Abstractions;

/// <summary>
/// Base class for a domain entity: something with an identity that is tracked
/// through its lifecycle (as opposed to a value object, which is defined only by
/// its attributes). Two entities are equal when they are the same runtime type and
/// share the same <see cref="Id"/>.
/// </summary>
/// <remarks>
/// Deliberately minimal — it defines identity and equality and nothing else. No
/// domain events, change tracking, auditing, timestamps or persistence concerns
/// live here, and it takes no framework dependency. Aggregate-specific behaviour
/// arrives with <c>AggregateRoot</c> in a later story.
/// </remarks>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : StronglyTypedId
{
    public TId Id { get; protected set; } = default!;

    public bool Equals(Entity<TId>? other)
        => other is not null
           && GetType() == other.GetType()
           && Id == other.Id;

    public override bool Equals(object? obj)
        => obj is Entity<TId> other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !Equals(left, right);
}
