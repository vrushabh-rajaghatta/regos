using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.AddComponent;

/// <param name="ParentComponentId">
/// Null for what the patient is handed; set for something inside it.
/// </param>
public sealed record AddComponentCommand(
    MedicinalProductId MedicinalProductId,
    MedicinalProductComponentId? ParentComponentId,
    string ComponentTypeCode,
    string Name,
    string? Description,
    decimal Quantity,
    string? UnitOfPresentationCode,
    string? DoseFormCode);
