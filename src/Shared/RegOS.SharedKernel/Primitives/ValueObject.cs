namespace RegOS.SharedKernel.Primitives;

/// <summary>
/// Base class for a value object: a concept defined entirely by its attributes,
/// with no identity. Two value objects are equal when they are the same runtime
/// type and all of their equality components are equal (an <c>Email</c> of
/// "john@company.com" equals any other <c>Email</c> of the same value).
/// </summary>
/// <remarks>
/// Value objects are expected to be immutable. Derived types declare which
/// members define equality by overriding <see cref="GetEqualityComponents"/>;
/// the base handles equality, operators and hashing from those components.
/// <see cref="System.Linq.Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})"/>
/// and <see cref="HashCode"/> both defer to the default equality comparer, so
/// null components and nested value objects are handled without special cases.
/// No identity, lifecycle, business logic or framework dependency lives here.
/// </remarks>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// The ordered set of members that define this value object's equality.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
        => other is not null
           && GetType() == other.GetType()
           && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override bool Equals(object? obj)
        => obj is ValueObject other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var component in GetEqualityComponents())
            hash.Add(component);

        return hash.ToHashCode();
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
        => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !Equals(left, right);
}
