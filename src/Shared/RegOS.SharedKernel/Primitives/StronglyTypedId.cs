namespace RegOS.SharedKernel.Primitives;

/// <summary>
/// Base class for a strongly typed identifier that wraps a single <see cref="Guid"/>.
/// Gives every id the same value-equality, hashing and string behaviour while each
/// module keeps its own distinct id type (an <c>OrganizationId</c> is never equal to a
/// <c>UserId</c>, even when they wrap the same <see cref="Guid"/>).
/// </summary>
/// <remarks>
/// The kernel deliberately keeps this minimal: no business logic and no external
/// framework dependencies. Because the id is an immutable value that exposes its
/// underlying <see cref="Guid"/>, it works directly as a dictionary key, as an EF Core
/// value-converter source, and for JSON serialization of the <see cref="Value"/>.
/// </remarks>
public abstract class StronglyTypedId : IEquatable<StronglyTypedId>
{
    protected StronglyTypedId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException(
                "A strongly typed id cannot be empty.",
                nameof(value));

        Value = value;
    }

    /// <summary>The underlying identifier value.</summary>
    public Guid Value { get; }

    public bool Equals(StronglyTypedId? other)
        => other is not null
           && GetType() == other.GetType()
           && Value.Equals(other.Value);

    public override bool Equals(object? obj)
        => obj is StronglyTypedId other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(GetType(), Value);

    public override string ToString() => Value.ToString();

    public static bool operator ==(StronglyTypedId? left, StronglyTypedId? right)
        => Equals(left, right);

    public static bool operator !=(StronglyTypedId? left, StronglyTypedId? right)
        => !Equals(left, right);
}
