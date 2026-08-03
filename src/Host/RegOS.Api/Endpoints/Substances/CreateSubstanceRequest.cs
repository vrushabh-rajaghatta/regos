namespace RegOS.Api.Endpoints.Substances;

/// <remarks>
/// No <c>tenantId</c> and no <c>isShared</c>. The route creates a tenant-owned
/// substance and nothing else, so there is no field a caller could set to ask
/// for a shared one (ADR-058 §5).
/// </remarks>
public sealed record CreateSubstanceRequest(
    string Name,
    string? Inn,
    string SubstanceClassCode,
    string SubstanceTypeCode,
    string? CasNumber,
    string? UniiCode,
    string? MolecularFormula,
    string? Description);
