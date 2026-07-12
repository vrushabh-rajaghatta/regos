namespace RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

public readonly record struct RegulatoryApplicationId(Guid Value)
{
    public static RegulatoryApplicationId New()
        => new(Guid.NewGuid());

    public override string ToString()
        => Value.ToString();
}
