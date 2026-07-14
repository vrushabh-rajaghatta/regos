namespace RegOS.ReferenceData.Domain.SubmissionType;

public readonly record struct SubmissionTypeId(Guid Value)
{
    public static SubmissionTypeId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SubmissionTypeId id)
        => id.Value;
}
