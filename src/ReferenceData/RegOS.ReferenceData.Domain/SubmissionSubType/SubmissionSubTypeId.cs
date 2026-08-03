namespace RegOS.ReferenceData.Domain.SubmissionSubType;

/// <remarks>
/// Flat master data — deterministic ids, no children, no lifecycle — so this is
/// a <c>readonly record struct</c> permanently (ADR-043 §2), like
/// <c>ApplicationTypeId</c> beside it. It is not one of the 15 ids pending
/// migration to <c>StronglyTypedId</c>.
/// </remarks>
public readonly record struct SubmissionSubTypeId(Guid Value)
{
    public static SubmissionSubTypeId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SubmissionSubTypeId id)
        => id.Value;
}
