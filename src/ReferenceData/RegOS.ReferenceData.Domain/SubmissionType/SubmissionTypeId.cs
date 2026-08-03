namespace RegOS.ReferenceData.Domain.SubmissionType;

/// <remarks>
/// Flat master data — deterministic ids, no children, no lifecycle — so this is
/// a <c>readonly record struct</c> permanently (ADR-043 §2), like
/// <c>ApplicationTypeId</c> beside it. It is not one of the 15 ids pending
/// migration to <c>StronglyTypedId</c>.
/// </remarks>
public readonly record struct SubmissionTypeId(Guid Value)
{
    public static SubmissionTypeId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SubmissionTypeId id)
        => id.Value;
}
