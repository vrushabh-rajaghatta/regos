using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Domain.Tests.Product;

/// <summary>
/// How long a pack keeps, and what has to be true of its storage for that to
/// hold.
/// </summary>
/// <remarks>
/// The sentence the type is built on: <em>"36 months"</em> alone is not a fact,
/// <em>"36 months below 25 °C"</em> is. Every test here is about keeping the
/// two halves of that statement in one object.
/// </remarks>
public sealed class ShelfLifeStorageTests
{
    private static readonly CodedConcept Months =
        SupplyVocabulary.ShelfLifePeriodOf("MONTH")!;

    private static readonly CodedConcept Years =
        SupplyVocabulary.ShelfLifePeriodOf("YEAR")!;

    private static readonly CodedConcept Below25 =
        SupplyVocabulary.StorageConditionOf("BELOW_25")!;

    private static readonly CodedConcept ProtectFromLight =
        SupplyVocabulary.StorageConditionOf("PROTECT_FROM_LIGHT")!;

    private static readonly CodedConcept NoneNeeded =
        SupplyVocabulary.StorageConditionOf(
            SupplyVocabulary.NoSpecialPrecautionsCode)!;

    // --- the statement -------------------------------------------------------

    [Fact]
    public void AShelfLifeIsAPeriodAndTheConditionsItHoldsUnder()
    {
        var statement = ShelfLifeStorage.Create(
            36, Months, "36 months.", [Below25, ProtectFromLight]);

        statement.Value.Should().Be(36);
        statement.Unit!.Display.Should().Be("months");
        statement.StorageConditions.Should().HaveCount(2);
        statement.IsStated.Should().BeTrue();
    }

    /// <summary>
    /// <b>Kept literal.</b> Three years is stored as three years — normalising
    /// it to thirty-six months would be the first unit conversion in RegOS, and
    /// a shelf life is quoted back on a label in the words it was approved in.
    /// </summary>
    [Fact]
    public void ThreeYearsIsNotThirtySixMonths()
    {
        var years = ShelfLifeStorage.Create(3, Years, null, []);
        var months = ShelfLifeStorage.Create(36, Months, null, []);

        years.Value.Should().Be(3);
        years.Unit!.Code.Should().Be("YEAR");
        years.Should().NotBe(months);
    }

    /// <summary>
    /// The period unit is drawn from the supply vocabulary and not the
    /// measurement one — otherwise <em>"500 months"</em> would be a legal
    /// strength.
    /// </summary>
    [Fact]
    public void APeriodIsNotAMeasurementUnit()
    {
        MeasurementVocabulary.UnitOf("MONTH").Should().BeNull();
        SupplyVocabulary.ShelfLifePeriodOf("MG").Should().BeNull();
    }

    // --- half a statement ----------------------------------------------------

    [Fact]
    public void ANumberWithNoPeriodIsRefused()
    {
        var create = () => ShelfLifeStorage.Create(36, null, null, []);

        create.Should().Throw<DomainException>()
            .WithMessage(ShelfLifeStorageErrors.PeriodUnitRequired);
    }

    [Fact]
    public void APeriodWithNoNumberIsRefused()
    {
        var create = () => ShelfLifeStorage.Create(null, Months, null, []);

        create.Should().Throw<DomainException>()
            .WithMessage(ShelfLifeStorageErrors.PeriodValueRequired);
    }

    [Fact]
    public void AShelfLifeOfNothingIsRefused()
    {
        var create = () => ShelfLifeStorage.Create(0, Months, null, []);

        create.Should().Throw<DomainException>()
            .WithMessage(ShelfLifeStorageErrors.PeriodMustBePositive);
    }

    // --- "none needed" is a conclusion, not a blank --------------------------

    /// <summary>
    /// <b>The distinction this vocabulary entry exists for.</b> An empty list
    /// means nobody has said; <em>"no special storage precautions"</em> means
    /// somebody checked. Those are different regulatory statements and the model
    /// keeps them apart.
    /// </summary>
    [Fact]
    public void SayingNoneAreNeededIsNotTheSameAsSayingNothing()
    {
        var silent = ShelfLifeStorage.Create(36, Months, null, []);
        var checkedAndNoneNeeded = ShelfLifeStorage.Create(
            36, Months, null, [NoneNeeded]);

        silent.NeedsNoSpecialPrecautions.Should().BeFalse();
        checkedAndNoneNeeded.NeedsNoSpecialPrecautions.Should().BeTrue();

        silent.Should().NotBe(checkedAndNoneNeeded);
    }

    [Fact]
    public void NoneNeededCannotSitBesideAPrecaution()
    {
        var create = () => ShelfLifeStorage.Create(
            36, Months, null, [NoneNeeded, Below25]);

        create.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ShelfLifeStorageErrors.NoSpecialPrecautionsStandsAlone);
    }

    /// <summary>
    /// Refused whichever order they arrive in — the rule is about the set, so
    /// it is checked once the set is complete rather than as each is added.
    /// </summary>
    [Fact]
    public void NoneNeededIsRefusedInEitherOrder()
    {
        var precautionFirst = () => ShelfLifeStorage.Create(
            36, Months, null, [Below25, NoneNeeded]);

        precautionFirst.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ShelfLifeStorageErrors.NoSpecialPrecautionsStandsAlone);
    }

    [Fact]
    public void TheSameConditionTwiceIsRefused()
    {
        var create = () => ShelfLifeStorage.Create(
            36, Months, null, [Below25, Below25]);

        create.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ShelfLifeStorageErrors.ConditionAlreadyStated);
    }

    // --- the empty statement -------------------------------------------------

    /// <summary>
    /// <b>A value, not a null.</b> It is also what makes the mapping safe: EF
    /// reads an optional owned reference back as null when every column it
    /// shares is null, which would take the storage conditions with it.
    /// </summary>
    [Fact]
    public void NotStatedIsAnEmptyStatementRatherThanAMissingOne()
    {
        var nothing = ShelfLifeStorage.NotStated;

        nothing.IsStated.Should().BeFalse();
        nothing.Value.Should().BeNull();
        nothing.StorageConditions.Should().BeEmpty();
    }

    /// <summary>
    /// A new instance each time: an owned instance belongs to exactly one owner
    /// in EF's change tracker, so a shared singleton would make the second pack
    /// to use it untrackable.
    /// </summary>
    [Fact]
    public void NotStatedHandsOutADistinctInstanceThatStillComparesEqual()
    {
        var first = ShelfLifeStorage.NotStated;
        var second = ShelfLifeStorage.NotStated;

        first.Should().NotBeSameAs(second);
        first.Should().Be(second);
    }

    /// <summary>
    /// Conditions alone, with no period, is an ordinary intermediate state — an
    /// SmPC's storage section is routinely settled before its shelf life is.
    /// </summary>
    [Fact]
    public void ConditionsWithNoPeriodAreAStatement()
    {
        var statement = ShelfLifeStorage.Create(null, null, null, [Below25]);

        statement.IsStated.Should().BeTrue();
        statement.Value.Should().BeNull();
    }

    // --- equality ------------------------------------------------------------

    /// <summary>
    /// Order is a fact about the form, not about the storage, so two statements
    /// naming the same precautions are equal however they were entered.
    /// </summary>
    [Fact]
    public void TheOrderConditionsWereEnteredInDoesNotChangeTheStatement()
    {
        var one = ShelfLifeStorage.Create(
            36, Months, "36 months.", [Below25, ProtectFromLight]);

        var other = ShelfLifeStorage.Create(
            36, Months, "36 months.", [ProtectFromLight, Below25]);

        one.Should().Be(other);
    }

    [Fact]
    public void DifferentConditionsAreDifferentStatements()
    {
        var one = ShelfLifeStorage.Create(36, Months, null, [Below25]);
        var other = ShelfLifeStorage.Create(36, Months, null, [ProtectFromLight]);

        one.Should().NotBe(other);
    }
}
