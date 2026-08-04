namespace RegOS.Api.Endpoints.MedicinalProducts;

/// <param name="AtcCode">
/// Null or blank clears the classification. RegOS checks the shape only — it
/// holds no WHO ATC index, so acceptance here is not verification.
/// </param>
public sealed record RecordAtcCodeRequest(string? AtcCode);
