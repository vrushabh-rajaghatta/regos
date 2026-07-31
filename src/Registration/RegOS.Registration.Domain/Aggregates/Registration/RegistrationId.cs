namespace RegOS.Registration.Domain.Aggregates.Registration;

public readonly record struct RegistrationId(Guid Value)
{
    public static RegistrationId New()
        => new(Guid.NewGuid());

    public override string ToString()
        => Value.ToString();
}
