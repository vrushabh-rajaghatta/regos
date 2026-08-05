using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Domain.Tests.Product;

/// <summary>
/// What the medicine looks like — <em>"White, round, debossed AZ 10."</em>
/// </summary>
/// <remarks>
/// On the <b>presentation</b>, which is ADR-061 §1's discriminator pointing the
/// other way for once: a tablet looks identical in a carton of 30 and a carton
/// of 100, so how it looks is part of what the medicine <em>is</em>.
/// </remarks>
public sealed class PhysicalCharacteristicsTests
{
    private static readonly CodedConcept White =
        PharmaceuticalVocabulary.ColourOf("WHITE")!;

    private static readonly CodedConcept Blue =
        PharmaceuticalVocabulary.ColourOf("BLUE")!;

    private static readonly CodedConcept Round =
        PharmaceuticalVocabulary.ShapeOf("ROUND")!;

    private static readonly CodedConcept Oval =
        PharmaceuticalVocabulary.ShapeOf("OVAL")!;

    // --- what an appearance is -----------------------------------------------

    /// <summary>
    /// <b>Several colours is ordinary, not exceptional.</b> A capsule with a
    /// white body and a blue cap is two colours — a single field would force
    /// either an invented "white and blue" vocabulary entry or prose.
    /// </summary>
    [Fact]
    public void ACapsuleMayBeTwoColours()
    {
        var appearance = PhysicalCharacteristics.Create(
            [White, Blue], null, null, "White body with a blue cap.");

        appearance.Colours.Should().HaveCount(2);
    }

    /// <summary>
    /// Shape is single-valued, unlike colour: a tablet is round or it is oval,
    /// and nothing is both.
    /// </summary>
    [Fact]
    public void AShapeIsOneShape()
    {
        var round = PhysicalCharacteristics.Create([], Round, null, null);
        var oval = PhysicalCharacteristics.Create([], Oval, null, null);

        round.Shape!.Display.Should().Be("Round");
        round.Should().NotBe(oval);
    }

    /// <summary>
    /// <b>Its own field rather than a phrase in the description</b>, because it
    /// is the one part of an appearance anybody looks a medicine <em>up</em> by
    /// — a poison centre with a loose tablet has the imprint and nothing else.
    /// </summary>
    [Fact]
    public void TheMarkingIsItsOwnFact()
    {
        var appearance = PhysicalCharacteristics.Create(
            [White], Round, "AZ 10", "White, round tablet debossed AZ 10.");

        appearance.Imprint.Should().Be("AZ 10");
        appearance.Description.Should().NotBe(appearance.Imprint);
    }

    [Fact]
    public void TheSameColourTwiceIsRefused()
    {
        var create = () => PhysicalCharacteristics.Create(
            [White, White], null, null, null);

        create.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(PhysicalCharacteristicsErrors.ColourAlreadyStated);
    }

    [Fact]
    public void AMarkingLongerThanAMarkingIsRefused()
    {
        var create = () => PhysicalCharacteristics.Create(
            [], null, new string('x', PhysicalCharacteristics.ImprintMaxLength + 1), null);

        create.Should().Throw<DomainException>()
            .WithMessage(PhysicalCharacteristicsErrors.ImprintTooLong);
    }

    // --- the empty statement -------------------------------------------------

    /// <summary>
    /// <b>The second use of the shape <c>ShelfLifeStorage</c> introduced</b>, and
    /// for the identical reason: an optional owned reference whose columns are
    /// all null is read back as null, taking the colours with it.
    /// </summary>
    [Fact]
    public void NotStatedIsAnEmptyStatementRatherThanAMissingOne()
    {
        var nothing = PhysicalCharacteristics.NotStated;

        nothing.IsStated.Should().BeFalse();
        nothing.Colours.Should().BeEmpty();
        nothing.Shape.Should().BeNull();
    }

    [Fact]
    public void NotStatedHandsOutADistinctInstanceThatStillComparesEqual()
    {
        PhysicalCharacteristics.NotStated
            .Should().NotBeSameAs(PhysicalCharacteristics.NotStated);

        PhysicalCharacteristics.NotStated
            .Should().Be(PhysicalCharacteristics.NotStated);
    }

    /// <summary>
    /// Colour alone is a statement — the exact shape that would be lost if the
    /// owned reference were optional, because every column it shares is null.
    /// </summary>
    [Fact]
    public void AColourAloneIsAStatement()
    {
        var appearance = PhysicalCharacteristics.Create([White], null, null, null);

        appearance.IsStated.Should().BeTrue();
        appearance.Shape.Should().BeNull();
        appearance.Imprint.Should().BeNull();
        appearance.Description.Should().BeNull();
    }

    // --- equality ------------------------------------------------------------

    [Fact]
    public void TheOrderColoursWereEnteredInDoesNotChangeTheAppearance()
    {
        PhysicalCharacteristics.Create([White, Blue], Round, null, null)
            .Should()
            .Be(PhysicalCharacteristics.Create([Blue, White], Round, null, null));
    }
}
