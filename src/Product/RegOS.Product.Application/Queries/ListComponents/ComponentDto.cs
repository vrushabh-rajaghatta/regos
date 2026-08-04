using RegOS.Product.Application.Queries.ListPresentations;

namespace RegOS.Product.Application.Queries.ListComponents;

/// <param name="ParentComponentId">
/// Null for what the patient is handed. The client builds the tree from this.
/// </param>
/// <param name="Depth">
/// One for a top-level article. Sent rather than derived on the client because
/// the server already walked the tree to order the rows, and two implementations
/// of the same walk is one more than the rule allows.
/// </param>
public sealed record ComponentDto(
    Guid ComponentId,
    Guid MedicinalProductId,
    Guid? ParentComponentId,
    int Depth,
    CodedValueDto ComponentType,
    string Name,
    string? Description,
    decimal Quantity,
    CodedValueDto? UnitOfPresentation,
    CodedValueDto? DoseForm);
