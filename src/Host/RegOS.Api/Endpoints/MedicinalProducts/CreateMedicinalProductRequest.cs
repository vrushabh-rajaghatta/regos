namespace RegOS.Api.Endpoints.MedicinalProducts;

/// <param name="StatusDate">
/// The business date this market presence began — supplied, not taken from the
/// clock, so a carried-over portfolio can state when it actually entered.
/// </param>
public sealed record CreateMedicinalProductRequest(
    Guid CountryId,
    DateOnly StatusDate);
