using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Tests.Product;

public class PharmaceuticalProductDetailTests
{
    private static CodedConcept Tablet() =>
        CodedConcept.Internal("TABLET", "Tablet");

    private static CodedConcept Oral() =>
        CodedConcept.Internal("ORAL", "Oral");

    private static CodedConcept Intravenous() =>
        CodedConcept.Internal("INTRAVENOUS", "Intravenous");

    private static PharmaceuticalProductDetail Presentation(
        string name = "Film-coated tablet",
        CodedConcept? unitOfPresentation = null,
        params CodedConcept[] routes)
        => PharmaceuticalProductDetail.Create(
            TenantId.From(Guid.NewGuid()),
            MedicinalProductId.New(),
            name,
            description: null,
            Tablet(),
            unitOfPresentation,
            routes);

    [Fact]
    public void APresentationBelongsToOneMarketAndOneTenant()
    {
        var tenantId = TenantId.From(Guid.NewGuid());
        var marketId = MedicinalProductId.New();

        var presentation = PharmaceuticalProductDetail.Create(
            tenantId, marketId, "Tablet", null, Tablet(), null, []);

        presentation.TenantId.Should().Be(tenantId);
        presentation.MedicinalProductId.Should().Be(marketId);
    }

    [Fact]
    public void ATenantIsRequired()
    {
        var act = () => PharmaceuticalProductDetail.Create(
            null!, MedicinalProductId.New(), "Tablet", null, Tablet(), null, []);

        act.Should().Throw<DomainException>()
            .WithMessage(PharmaceuticalProductDetailErrors.TenantRequired);
    }

    [Fact]
    public void AMarketIsRequired()
    {
        var act = () => PharmaceuticalProductDetail.Create(
            TenantId.From(Guid.NewGuid()), null!, "Tablet", null, Tablet(), null, []);

        act.Should().Throw<DomainException>()
            .WithMessage(PharmaceuticalProductDetailErrors.MarketRequired);
    }

    [Fact]
    public void ADoseFormIsRequired()
    {
        var act = () => PharmaceuticalProductDetail.Create(
            TenantId.From(Guid.NewGuid()),
            MedicinalProductId.New(),
            "Tablet",
            null,
            null!,
            null,
            []);

        act.Should().Throw<DomainException>()
            .WithMessage(PharmaceuticalProductDetailErrors.DoseFormRequired);
    }

    /// <summary>
    /// A solution for injection is routinely intravenous <em>and</em>
    /// intramuscular. Several routes is the ordinary case, not the exception.
    /// </summary>
    [Fact]
    public void SeveralRoutesAreOrdinary()
    {
        var presentation = Presentation(
            unitOfPresentation: null, routes: [Oral(), Intravenous()]);

        presentation.RoutesOfAdministration
            .Select(x => x.Code)
            .Should().Equal("ORAL", "INTRAVENOUS");
    }

    /// <summary>
    /// No route is ordinary too — a presentation may be recorded before the
    /// route is settled.
    /// </summary>
    [Fact]
    public void NoRouteIsAllowed()
    {
        Presentation().RoutesOfAdministration.Should().BeEmpty();
    }

    /// <summary>
    /// The same route twice is not extra information; it is one fact rendered
    /// twice in every downstream listing. Compared by value, because each
    /// concept must be its own object to be persisted against its own owner.
    /// </summary>
    [Fact]
    public void TheSameRouteTwiceIsRefused()
    {
        var act = () => Presentation(
            unitOfPresentation: null, routes: [Oral(), Oral()]);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(PharmaceuticalProductDetailErrors.RouteAlreadyRecorded);
    }

    /// <summary>
    /// The unit of presentation is optional: an oral solution measured in mL
    /// has no natural article to count.
    /// </summary>
    [Fact]
    public void AUnitOfPresentationIsOptional()
    {
        Presentation().UnitOfPresentation.Should().BeNull();
    }

    /// <summary>
    /// Restate replaces the routes rather than adding to them — which is why
    /// the repository must load them, and why a partial update was not offered.
    /// </summary>
    [Fact]
    public void RestateReplacesTheWholeStatement()
    {
        var presentation = Presentation(
            unitOfPresentation: null, routes: [Oral(), Intravenous()]);

        presentation.Restate(
            "Solution for injection",
            "Corrected after the formulation change",
            CodedConcept.Internal("SOLUTION_FOR_INJECTION", "Solution for injection"),
            CodedConcept.Internal("VIAL", "Vial"),
            [Intravenous()]);

        presentation.Name.Should().Be("Solution for injection");
        presentation.Description.Should().Be("Corrected after the formulation change");
        presentation.DoseForm.Code.Should().Be("SOLUTION_FOR_INJECTION");
        presentation.UnitOfPresentation!.Code.Should().Be("VIAL");
        presentation.RoutesOfAdministration
            .Select(x => x.Code).Should().Equal("INTRAVENOUS");
    }

    [Fact]
    public void RestateCanClearTheUnitAndTheDescription()
    {
        var presentation = PharmaceuticalProductDetail.Create(
            TenantId.From(Guid.NewGuid()),
            MedicinalProductId.New(),
            "Tablet",
            "A description",
            Tablet(),
            CodedConcept.Internal("TABLET", "Tablet"),
            [Oral()]);

        presentation.Restate("Tablet", null, Tablet(), null, [Oral()]);

        presentation.Description.Should().BeNull();
        presentation.UnitOfPresentation.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ANameIsRequired(string? name)
    {
        var act = () => Presentation(name!);

        act.Should().Throw<DomainException>()
            .WithMessage(PharmaceuticalProductDetailErrors.NameRequired);
    }

    [Fact]
    public void ANameIsTrimmed()
    {
        Presentation("  Tablet  ").Name.Should().Be("Tablet");
    }

    [Fact]
    public void ANameTooLongIsRefused()
    {
        var act = () => Presentation(
            new string('x', PharmaceuticalProductDetail.NameMaxLength + 1));

        act.Should().Throw<DomainException>()
            .WithMessage(PharmaceuticalProductDetailErrors.NameTooLong);
    }

    /// <summary>
    /// A blank description is absence, not an empty string — only one of the
    /// two can be queried for.
    /// </summary>
    [Fact]
    public void ABlankDescriptionCollapsesToNull()
    {
        PharmaceuticalProductDetail.Create(
                TenantId.From(Guid.NewGuid()),
                MedicinalProductId.New(),
                "Tablet",
                "   ",
                Tablet(),
                null,
                [])
            .Description.Should().BeNull();
    }
    // --- what it looks like --------------------------------------------------

    /// <summary>
    /// <b>Never null.</b> A presentation nobody has described carries the empty
    /// statement, so no caller has to guard the navigation.
    /// </summary>
    [Fact]
    public void ANewPresentationCarriesAnEmptyAppearanceRatherThanNone()
    {
        var presentation = APresentation();

        presentation.Appearance.Should().NotBeNull();
        presentation.Appearance.IsStated.Should().BeFalse();
    }

    [Fact]
    public void AnAppearanceIsDescribedWhole()
    {
        var presentation = APresentation();

        presentation.DescribeAppearance(PhysicalCharacteristics.Create(
            [PharmaceuticalVocabulary.ColourOf("WHITE")!],
            PharmaceuticalVocabulary.ShapeOf("ROUND"),
            "AZ 10",
            "White, round tablet debossed AZ 10."));

        presentation.Appearance.Imprint.Should().Be("AZ 10");
        presentation.Appearance.Colours.Should().ContainSingle();
    }

    /// <summary>
    /// <c>NotStated</c> withdraws a description; null is a caller mistake,
    /// because a presentation always has one.
    /// </summary>
    [Fact]
    public void AnAppearanceCanBeWithdrawnButNotNulled()
    {
        var presentation = APresentation();

        presentation.DescribeAppearance(
            PhysicalCharacteristics.Create([], null, "AZ 10", null));

        presentation.DescribeAppearance(PhysicalCharacteristics.NotStated);
        presentation.Appearance.IsStated.Should().BeFalse();

        var nulled = () => presentation.DescribeAppearance(null!);

        nulled.Should().Throw<DomainException>()
            .WithMessage(PhysicalCharacteristicsErrors.AppearanceRequired);
    }

    /// <summary>
    /// A presentation is recorded when its dose form is known and described
    /// when somebody has seen it — routinely later, and by somebody else. So
    /// correcting the trade-name-era facts leaves the appearance alone.
    /// </summary>
    [Fact]
    public void RestatingAPresentationDoesNotDisturbItsAppearance()
    {
        var presentation = APresentation();

        presentation.DescribeAppearance(
            PhysicalCharacteristics.Create([], null, "AZ 10", null));

        presentation.Restate(
            "Film-coated tablet, 20 mg", null, Tablet(), null, []);

        presentation.Appearance.Imprint.Should().Be("AZ 10");
    }

    private static PharmaceuticalProductDetail APresentation()
        => PharmaceuticalProductDetail.Create(
            TenantId.From(Guid.NewGuid()),
            MedicinalProductId.New(),
            "Film-coated tablet, 10 mg",
            null,
            Tablet(),
            null,
            []);

}
