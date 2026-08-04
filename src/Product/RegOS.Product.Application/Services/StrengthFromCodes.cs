using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Services;

/// <summary>
/// Turns the numbers and unit codes on the wire into a <see cref="Strength"/>,
/// refusing any unit RegOS does not know.
/// </summary>
/// <remarks>
/// "Is this a word?" is answered at the boundary, not by the aggregate — the
/// same division <c>PresentationVocabulary</c> draws for dose forms and routes.
/// <para>
/// <b>Both units come from <see cref="MeasurementVocabulary"/>, and neither can
/// come from the presentation's list of articles.</b> That is what keeps a
/// strength orthogonal to the presentation it sits in: <em>"500 mg per
/// tablet"</em> is not expressible, because the presentation already says
/// tablet.
/// </para>
/// </remarks>
internal static class StrengthFromCodes
{
    public static Strength? Create(
        decimal? numeratorValue,
        string? numeratorUnitCode,
        decimal? denominatorValue,
        string? denominatorUnitCode)
    {
        // No strength at all is a legitimate answer for an excipient. The
        // aggregate decides whether this row was allowed to omit it.
        if (numeratorValue is null && string.IsNullOrWhiteSpace(numeratorUnitCode))
            return null;

        if (numeratorValue is null)
            throw new DomainException(StrengthErrors.NumeratorMustBePositive);

        return Strength.Create(
            numeratorValue.Value,
            Unit(numeratorUnitCode),
            denominatorValue,
            string.IsNullOrWhiteSpace(denominatorUnitCode)
                ? null
                : Unit(denominatorUnitCode));
    }

    private static CodedConcept Unit(string? code)
        => MeasurementVocabulary.UnitOf(code)
            ?? throw new DomainException(MeasurementErrors.UnknownUnit(code));
}
