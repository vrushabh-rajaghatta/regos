using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.RestateComponent;

/// <remarks>
/// No parent — moving a component is the operation with a rule attached, and
/// folding it in here would let a caller change the tree's shape without
/// passing the tree.
/// </remarks>
public sealed record RestateComponentCommand(
    MedicinalProductComponentId ComponentId,
    string ComponentTypeCode,
    string Name,
    string? Description,
    decimal Quantity,
    string? UnitOfPresentationCode,
    string? DoseFormCode);
