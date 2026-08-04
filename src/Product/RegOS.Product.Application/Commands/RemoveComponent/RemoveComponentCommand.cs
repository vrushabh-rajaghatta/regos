using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.RemoveComponent;

public sealed record RemoveComponentCommand(
    MedicinalProductComponentId ComponentId);
