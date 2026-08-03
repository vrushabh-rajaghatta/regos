namespace RegOS.Persistence.Initialization.ReferenceData;

internal static class SubstanceIds
{
    // Substances (b000...). Shared catalogue rows only; a tenant's own
    // compounds get fresh guids at creation.
    public static readonly Guid Paracetamol =
        Guid.Parse("b0000000-0000-0000-0000-000000000001");
    public static readonly Guid Ibuprofen =
        Guid.Parse("b0000000-0000-0000-0000-000000000002");
    public static readonly Guid Amoxicillin =
        Guid.Parse("b0000000-0000-0000-0000-000000000003");
    public static readonly Guid Metformin =
        Guid.Parse("b0000000-0000-0000-0000-000000000004");
    public static readonly Guid Aspirin =
        Guid.Parse("b0000000-0000-0000-0000-000000000005");
    public static readonly Guid Omeprazole =
        Guid.Parse("b0000000-0000-0000-0000-000000000006");
}
