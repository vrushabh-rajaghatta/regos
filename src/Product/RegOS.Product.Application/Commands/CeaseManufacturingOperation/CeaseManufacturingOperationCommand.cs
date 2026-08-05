using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.CeaseManufacturingOperation;

/// <summary>
/// Closes the period — the site no longer performs this operation.
/// </summary>
/// <remarks>
/// <b>Closed, never deleted</b> (ES-018). A transfer is this followed by a new
/// operation, and the pair reads as the history it is: <em>"who released our
/// batches in 2023?"</em> stays answerable.
/// </remarks>
public sealed record CeaseManufacturingOperationCommand(
    ManufacturingOperationId ManufacturingOperationId,
    DateOnly CeasedOn);
