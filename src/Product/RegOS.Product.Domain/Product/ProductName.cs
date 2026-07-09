public sealed record ProductName
{
    public ProductName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }
    public string Value { get; }
}