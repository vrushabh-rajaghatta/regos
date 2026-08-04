using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// How much of a substance a presentation contains — <em>500 mg</em>, or
/// <em>10 mg per 1 mL</em>.
/// </summary>
/// <remarks>
/// <b>Never a string.</b> Storing <c>"50mg/5ml"</c> as text makes every
/// downstream comparison, conversion and xEVMPD render impossible, and the
/// first thing anyone would do with it is parse it back (EPIC-010a D4).
/// <para>
/// <b>Orthogonal to presentation, and the unit vocabulary is what enforces
/// that.</b> Both units come from <see cref="MeasurementVocabulary"/> —
/// quantities — never from the presentation's list of countable articles. So
/// <em>"500 mg per tablet"</em> cannot be expressed here, and does not need to
/// be: <c>500 mg</c> on a presentation whose dose form is <em>Tablet</em>
/// already says it. Letting the denominator name an article would state the
/// same fact twice, in two places that can disagree.
/// </para>
/// <para>
/// <b>The denominator is optional, and its absence is the point strength.</b>
/// <c>500 mg</c> has none. <c>10 mg / 1 mL</c> has one, because for a solution
/// the volume is genuinely part of the strength rather than part of the
/// packaging.
/// </para>
/// <para>
/// <b>No arithmetic here yet.</b> Comparing <c>10 mg/mL</c> with
/// <c>1 g/100 mL</c> needs a unit conversion table RegOS does not hold, and a
/// half-built one that silently handles mass but not activity units would be
/// worse than none. The shape is what makes that table addable later without
/// touching a caller.
/// </para>
/// </remarks>
public sealed class Strength : ValueObject
{
    private Strength()
    {
    }

    public decimal NumeratorValue { get; private init; }

    public CodedConcept NumeratorUnit { get; private init; } = default!;

    /// <summary>Null for a point strength, set for a concentration.</summary>
    public decimal? DenominatorValue { get; private init; }

    public CodedConcept? DenominatorUnit { get; private init; }

    /// <summary>
    /// True when this expresses an amount per volume rather than an amount.
    /// </summary>
    public bool IsConcentration => DenominatorValue is not null;

    public static Strength Create(
        decimal numeratorValue,
        CodedConcept numeratorUnit,
        decimal? denominatorValue = null,
        CodedConcept? denominatorUnit = null)
    {
        if (numeratorValue <= 0)
            throw new DomainException(StrengthErrors.NumeratorMustBePositive);

        if (numeratorUnit is null)
            throw new DomainException(StrengthErrors.NumeratorUnitRequired);

        // Half a fraction is not a smaller fraction, it is a broken one. A
        // value with no unit cannot be read, and a unit with no value cannot be
        // measured — so the pair travels together or not at all.
        if (denominatorValue is not null && denominatorUnit is null)
            throw new DomainException(StrengthErrors.DenominatorUnitRequired);

        if (denominatorUnit is not null && denominatorValue is null)
            throw new DomainException(StrengthErrors.DenominatorValueRequired);

        if (denominatorValue is <= 0)
            throw new DomainException(StrengthErrors.DenominatorMustBePositive);

        return new Strength
        {
            NumeratorValue = numeratorValue,
            NumeratorUnit = numeratorUnit,
            DenominatorValue = denominatorValue,
            DenominatorUnit = denominatorUnit
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return NumeratorValue;
        yield return NumeratorUnit;
        yield return DenominatorValue;
        yield return DenominatorUnit;
    }

    /// <remarks>
    /// For logs and diagnostics. The screen composes its own — a UI that reads
    /// this would inherit a format decision made here for a different reason.
    /// </remarks>
    public override string ToString()
        => IsConcentration
            ? $"{NumeratorValue} {NumeratorUnit.Display} / {DenominatorValue} {DenominatorUnit!.Display}"
            : $"{NumeratorValue} {NumeratorUnit.Display}";
}
