namespace RegOS.ReferenceData.Domain.Organization;

public readonly record struct ContactRoleId(Guid Value)
{
    public static ContactRoleId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ContactRoleId id) => id.Value;
}
