namespace RegOS.Api.Endpoints.Components;

/// <param name="ParentComponentId">
/// Null for what the patient is handed; set for something inside it.
/// </param>
public sealed record AddComponentRequest(
    Guid? ParentComponentId,
    string ComponentTypeCode,
    string Name,
    string? Description,
    decimal Quantity,
    string? UnitOfPresentationCode,
    string? DoseFormCode);

/// <remarks>
/// No parent. Moving a component is its own route, because it is the operation
/// with the rules attached — folding it in here would let a caller change the
/// tree's shape through a general update.
/// </remarks>
public sealed record RestateComponentRequest(
    string ComponentTypeCode,
    string Name,
    string? Description,
    decimal Quantity,
    string? UnitOfPresentationCode,
    string? DoseFormCode);

/// <param name="NewParentComponentId">Null moves it to the top level.</param>
public sealed record MoveComponentRequest(Guid? NewParentComponentId);

public sealed record AddComponentResponse(Guid Id);
