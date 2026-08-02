namespace RegOS.ReferenceData.Domain.ApplicationType;

/// <remarks>
/// Flat master data — deterministic ids, no children, no lifecycle — so this
/// stays a <c>readonly record struct</c> permanently (ADR-043 §2). It is not
/// one of the ids pending migration to <c>StronglyTypedId</c>, and renaming it
/// is not the moment to convert it.
/// </remarks>
public readonly record struct ApplicationTypeId(Guid Value)
{
    public static ApplicationTypeId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ApplicationTypeId id)
        => id.Value;
}
