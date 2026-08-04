namespace RegOS.Product.Application.Queries.ListPresentations;

/// <param name="System">
/// Sent, not hidden. Every term RegOS ships today is its own, and a screen
/// showing "Tablet" without saying whose word it is implies terminology the
/// platform does not hold (ADR-058 §6).
/// </param>
public sealed record CodedValueDto(
    string System,
    string Code,
    string Display);

public sealed record PresentationDto(
    Guid PresentationId,
    Guid MedicinalProductId,
    string Name,
    string? Description,
    CodedValueDto DoseForm,
    CodedValueDto? UnitOfPresentation,
    IReadOnlyList<CodedValueDto> RoutesOfAdministration);
