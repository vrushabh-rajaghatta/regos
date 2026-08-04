using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.MoveComponent;

/// <param name="NewParentComponentId">
/// Null moves it to the top level — out of whatever was holding it.
/// </param>
public sealed record MoveComponentCommand(
    MedicinalProductComponentId ComponentId,
    MedicinalProductComponentId? NewParentComponentId);
