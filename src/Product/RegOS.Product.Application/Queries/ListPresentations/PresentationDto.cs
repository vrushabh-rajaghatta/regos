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

/// <param name="HasAnActiveIngredient">
/// Whether the composition says what the product works by. A completeness fact,
/// not a validity one — an unfinished composition is ordinary, and the screen
/// says so rather than the write path refusing it.
/// </param>
public sealed record PresentationDto(
    Guid PresentationId,
    Guid MedicinalProductId,
    string Name,
    string? Description,
    CodedValueDto DoseForm,
    CodedValueDto? UnitOfPresentation,
    IReadOnlyList<CodedValueDto> RoutesOfAdministration,
    IReadOnlyList<IngredientDto> Ingredients,
    bool HasAnActiveIngredient,
    AppearanceDto Appearance);

/// <summary>
/// What the medicine looks like. Screen word: <b>Appearance</b>.
/// </summary>
/// <remarks>
/// <b>Never null</b>, because the presentation always carries a statement —
/// <paramref name="IsStated"/> says whether anybody has filled it in. A
/// presentation nobody has described and one described as having no stated
/// appearance are the same thing here, unlike storage conditions, where
/// "checked, none needed" is its own claim.
/// </remarks>
public sealed record AppearanceDto(
    IReadOnlyList<CodedValueDto> Colours,
    CodedValueDto? Shape,
    string? Imprint,
    string? Description,
    bool IsStated);

/// <param name="SubstanceName">
/// Joined from the substance catalogue, never copied onto the ingredient. The
/// row stores an id precisely so a substance renamed in one place is renamed
/// everywhere it appears.
/// </param>
/// <param name="Strength">
/// Null when nothing was declared — routine for an excipient, and refused by
/// the domain for an active.
/// </param>
public sealed record IngredientDto(
    Guid IngredientId,
    Guid SubstanceId,
    string SubstanceName,
    string? SubstanceInn,
    string Role,
    StrengthDto? Strength);

/// <param name="DenominatorValue">
/// Null for a point strength — <em>500 mg</em>. Set for a concentration —
/// <em>10 mg per 1 mL</em> — where the volume is part of the strength rather
/// than part of the packaging.
/// </param>
public sealed record StrengthDto(
    decimal NumeratorValue,
    CodedValueDto NumeratorUnit,
    decimal? DenominatorValue,
    CodedValueDto? DenominatorUnit);
