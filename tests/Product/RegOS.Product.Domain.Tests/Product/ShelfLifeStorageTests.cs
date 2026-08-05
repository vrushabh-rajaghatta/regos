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

    private static readonly CodedConcept Temperate =
        StabilityVocabulary.ConditionOf("25C_60RH")!;

    private static readonly CodedConcept HotHumid =
        StabilityVocabulary.ConditionOf("30C_75RH")!;

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

    // --- what the shelf life was demonstrated under --------------------------

    /// <summary>
    /// <b>Two lists, one field apart, and they are not the same thing.</b>
    /// <c>StorageConditions</c> is a label instruction addressed to whoever
    /// holds the pack; <c>TestedAt</c> is the condition a stability study was
    /// run at. A pack labelled <em>"do not store above 25 °C"</em> is routinely
    /// tested at 30 °C/75% RH, and neither can be derived from the other —
    /// which is exactly what this asserts.
    /// </summary>
    [Fact]
    public void HowItIsKeptAndWhatItWasTestedAtAreDifferentStatements()
    {
        var statement = ShelfLifeStorage.Create(
            36, Months, null, [Below25], [HotHumid]);

        statement.StorageConditions.Select(x => x.Code).Should()
            .BeEquivalentTo(["BELOW_25"]);
        statement.TestedAt.Select(x => x.Code).Should()
            .BeEquivalentTo(["30C_75RH"]);
    }

    /// <summary>
    /// <b>Several, because a global programme runs several.</b> Long-term data
    /// at both 25 °C/60% RH and 30 °C/75% RH supports temperate and hot-humid
    /// markets from one submission.
    /// </summary>
    [Fact]
    public void AShelfLifeMayHaveBeenDemonstratedAtSeveralConditions()
    {
        ShelfLifeStorage.Create(36, Months, null, [], [Temperate, HotHumid])
            .TestedAt.Should().HaveCount(2);
    }

    [Fact]
    public void TheSameTestingConditionTwiceIsRefused()
    {
        var create = () =>
            ShelfLifeStorage.Create(36, Months, null, [], [Temperate, Temperate]);

        create.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ShelfLifeStorageErrors.TestedAtAlreadyStated);
    }

    /// <summary>
    /// <b>No rule ties the two lists together, and none is missing.</b> A pack
    /// may say where its data came from before anybody has decided how long it
    /// keeps — a stability programme reports the condition first — and a
    /// 30 °C/75% RH study is what supports a "below 25 °C" label in a hot
    /// market.
    /// </summary>
    [Fact]
    public void ATestingConditionWithNoPeriodIsAStatement()
    {
        var statement = ShelfLifeStorage.Create(null, null, null, [], [Temperate]);

        statement.IsStated.Should().BeTrue();
        statement.Value.Should().BeNull();
    }

    /// <summary>
    /// Empty is ordinary: the stability data has not been recorded, which is
    /// not a rejection. <c>NotStated</c> carries neither collection.
    /// </summary>
    [Fact]
    public void NotStatedHasBeenTestedAtNothing()
    {
        ShelfLifeStorage.NotStated.TestedAt.Should().BeEmpty();
        ShelfLifeStorage.NotStated.IsStated.Should().BeFalse();
    }

    /// <summary>
    /// <b>The two collections cannot blur into one sequence.</b> Equality walks
    /// both, and a separator between them is what stops a statement storing one
    /// condition from comparing equal to a statement tested at another.
    /// </summary>
    [Fact]
    public void TestingConditionsChangeTheStatement()
    {
        var temperate = ShelfLifeStorage.Create(36, Months, null, [], [Temperate]);
        var hotHumid = ShelfLifeStorage.Create(36, Months, null, [], [HotHumid]);
        var both = ShelfLifeStorage.Create(36, Months, null, [], [HotHumid, Temperate]);

        temperate.Should().NotBe(hotHumid);
        temperate.Should().NotBe(both);

        // Order is a fact about the form, not about the study.
        both.Should().Be(
            ShelfLifeStorage.Create(36, Months, null, [], [Temperate, HotHumid]));
    }
}
