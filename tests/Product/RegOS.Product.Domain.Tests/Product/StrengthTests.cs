using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Domain.Tests.Product;

public class StrengthTests
{
    private static CodedConcept Mg() => CodedConcept.Internal("MG", "mg");

    private static CodedConcept Ml() => CodedConcept.Internal("ML", "mL");

    /// <summary>
    /// The founder's <c>{Value, Unit}</c> shape — a point strength is a
    /// strength with no denominator, not a different type.
    /// </summary>
    [Fact]
    public void APointStrengthHasNoDenominator()
    {
        var strength = Strength.Create(500m, Mg());

        strength.NumeratorValue.Should().Be(500m);
        strength.NumeratorUnit.Code.Should().Be("MG");
        strength.DenominatorValue.Should().BeNull();
        strength.IsConcentration.Should().BeFalse();
    }

    /// <summary>
    /// The case a point strength cannot express, and the reason the
    /// denominator exists: for a solution the volume is part of the strength
    /// rather than part of the packaging.
    /// </summary>
    [Fact]
    public void AConcentrationCarriesBoth()
    {
        var strength = Strength.Create(10m, Mg(), 1m, Ml());

        strength.IsConcentration.Should().BeTrue();
        strength.DenominatorValue.Should().Be(1m);
        strength.DenominatorUnit!.Code.Should().Be("ML");
    }

    /// <summary>
    /// Half a fraction is not a smaller fraction, it is a broken one.
    /// </summary>
    [Fact]
    public void ADenominatorValueWithoutAUnitIsRefused()
    {
        var act = () => Strength.Create(10m, Mg(), 5m);

        act.Should().Throw<DomainException>()
            .WithMessage(StrengthErrors.DenominatorUnitRequired);
    }

    [Fact]
    public void ADenominatorUnitWithoutAValueIsRefused()
    {
        var act = () => Strength.Create(10m, Mg(), null, Ml());

        act.Should().Throw<DomainException>()
            .WithMessage(StrengthErrors.DenominatorValueRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ANonPositiveStrengthIsRefused(decimal value)
    {
        var act = () => Strength.Create(value, Mg());

        act.Should().Throw<DomainException>()
            .WithMessage(StrengthErrors.NumeratorMustBePositive);
    }

    [Fact]
    public void ANonPositiveDenominatorIsRefused()
    {
        var act = () => Strength.Create(10m, Mg(), 0m, Ml());

        act.Should().Throw<DomainException>()
            .WithMessage(StrengthErrors.DenominatorMustBePositive);
    }

    [Fact]
    public void AUnitIsRequired()
    {
        var act = () => Strength.Create(10m, null!);

        act.Should().Throw<DomainException>()
            .WithMessage(StrengthErrors.NumeratorUnitRequired);
    }

    [Fact]
    public void TwoStrengthsWithTheSameNumbersAndUnitsAreEqual()
    {
        Strength.Create(10m, Mg(), 1m, Ml())
            .Should().Be(Strength.Create(10m, Mg(), 1m, Ml()));
    }

    /// <summary>
    /// Deliberately <b>not</b> equal, and this is the limitation worth
    /// knowing: RegOS holds no unit conversion table, so equality is literal.
    /// A half-built one that handled mass but not activity units would be
    /// worse than none.
    /// </summary>
    [Fact]
    public void EquivalentStrengthsInDifferentUnitsAreNotEqual()
    {
        var perMl = Strength.Create(10m, Mg(), 1m, Ml());
        var perLitre = Strength.Create(
            10_000m, Mg(), 1m, CodedConcept.Internal("L", "L"));

        perLitre.Should().NotBe(perMl);
    }

    [Fact]
    public void APointStrengthAndAConcentrationAreNotEqual()
    {
        Strength.Create(10m, Mg(), 1m, Ml())
            .Should().NotBe(Strength.Create(10m, Mg()));
    }
}
