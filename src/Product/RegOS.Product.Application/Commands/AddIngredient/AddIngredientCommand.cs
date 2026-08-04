using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Substances;

namespace RegOS.Product.Application.Commands.AddIngredient;

/// <param name="SubstanceId">
/// The shared fact, not a name. This is the field that makes <em>"which
/// products contain substance X?"</em> answerable backwards.
/// </param>
/// <param name="NumeratorUnitCode">
/// A code from <c>MeasurementVocabulary</c> — mg, mL, IU. Never a unit of
/// presentation: a strength measures a quantity, and the presentation already
/// says what article it comes in.
/// </param>
public sealed record AddIngredientCommand(
    PharmaceuticalProductDetailId PresentationId,
    SubstanceId SubstanceId,
    IngredientRole Role,
    decimal? NumeratorValue,
    string? NumeratorUnitCode,
    decimal? DenominatorValue,
    string? DenominatorUnitCode);
