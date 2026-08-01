namespace RegOS.Api.Endpoints.MedicinalProducts;

/// <param name="Language">An ISO 639-1 two-letter code, such as en or fr.</param>
public sealed record AddTradeNameRequest(string? Language, string? Name);
