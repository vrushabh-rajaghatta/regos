using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Substances;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Tests.Product;

/// <summary>
/// The composition rules, which live on the presentation rather than on one
/// ingredient because each of them reads the whole list.
/// </summary>
public class CompositionTests
{
    private static Strength Mg(decimal value)
        => Strength.Create(value, CodedConcept.Internal("MG", "mg"));

    private static PharmaceuticalProductDetail Presentation()
        => PharmaceuticalProductDetail.Create(
            TenantId.From(Guid.NewGuid()),
            MedicinalProductId.New(),
            "Film-coated tablet",
            null,
            CodedConcept.Internal("TABLET", "Tablet"),
            null,
            []);

    [Fact]
    public void APresentationStartsWithNoComposition()
    {
        var presentation = Presentation();

        presentation.Ingredients.Should().BeEmpty();
        presentation.HasAnActiveIngredient.Should().BeFalse();
    }

    [Fact]
    public void AnActiveIsRecordedWithItsStrength()
    {
        var presentation = Presentation();
        var substanceId = SubstanceId.New();

        var ingredient = presentation.AddIngredient(
            substanceId, IngredientRole.Active, Mg(500));

        ingredient.SubstanceId.Should().Be(substanceId);
        ingredient.Role.Should().Be(IngredientRole.Active);
        ingredient.Strength!.NumeratorValue.Should().Be(500m);
        presentation.HasAnActiveIngredient.Should().BeTrue();
    }

    /// <summary>
    /// A product works by its actives, so an active nobody has quantified is an
    /// incomplete formulation rather than a formulation with a blank.
    /// </summary>
    [Fact]
    public void AnActiveWithoutAStrengthIsRefused()
    {
        var presentation = Presentation();

        var act = () => presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Active, null);

        act.Should().Throw<DomainException>()
            .WithMessage(IngredientErrors.ActiveNeedsAStrength);
    }

    /// <summary>
    /// An excipient's quantity is routinely undeclared — <em>q.s.</em> — so its
    /// absence is a fact rather than a gap.
    /// </summary>
    [Fact]
    public void AnExcipientMayLeaveItsStrengthBlank()
    {
        var presentation = Presentation();

        var ingredient = presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Excipient, null);

        ingredient.Strength.Should().BeNull();
    }

    /// <summary>
    /// The same substance twice is one fact stated twice, and it would double
    /// every quantity a reader adds up.
    /// </summary>
    [Fact]
    public void TheSameSubstanceTwiceIsRefused()
    {
        var presentation = Presentation();
        var substanceId = SubstanceId.New();

        presentation.AddIngredient(substanceId, IngredientRole.Active, Mg(500));

        var act = () => presentation.AddIngredient(
            substanceId, IngredientRole.Excipient, null);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(IngredientErrors.SubstanceAlreadyInComposition);
    }

    /// <summary>
    /// Entry order is not dictated. Requiring an active on every edit would
    /// refuse a user who types the excipients first, and completeness belongs
    /// at a gate rather than on every keystroke.
    /// </summary>
    [Fact]
    public void AnExcipientMayBeRecordedFirst()
    {
        var presentation = Presentation();

        presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Excipient, null);

        presentation.Ingredients.Should().HaveCount(1);
        presentation.HasAnActiveIngredient.Should().BeFalse();
    }

    /// <summary>
    /// The anti-corruption rule: a formulation that has an active may not be
    /// left as a list of excipients describing nothing.
    /// </summary>
    [Fact]
    public void RemovingTheLastActiveIsRefusedWhileOthersRemain()
    {
        var presentation = Presentation();

        var active = presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Active, Mg(500));

        presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Excipient, null);

        var act = () => presentation.RemoveIngredient(active.Id);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(IngredientErrors.CompositionNeedsAnActive);
    }

    /// <summary>
    /// Emptying a composition entirely is starting over, which is a different
    /// act from hollowing one out.
    /// </summary>
    [Fact]
    public void RemovingTheOnlyIngredientOfAllIsAllowed()
    {
        var presentation = Presentation();

        var active = presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Active, Mg(500));

        presentation.RemoveIngredient(active.Id);

        presentation.Ingredients.Should().BeEmpty();
    }

    [Fact]
    public void RemovingAnActiveIsAllowedWhileAnotherRemains()
    {
        var presentation = Presentation();

        var first = presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Active, Mg(500));

        presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Active, Mg(200));

        presentation.RemoveIngredient(first.Id);

        presentation.Ingredients.Should().HaveCount(1);
        presentation.HasAnActiveIngredient.Should().BeTrue();
    }

    /// <summary>
    /// Demoting the last active hollows the composition out just as removing it
    /// would, so the same guard covers both.
    /// </summary>
    [Fact]
    public void DemotingTheLastActiveIsRefusedWhileOthersRemain()
    {
        var presentation = Presentation();

        var active = presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Active, Mg(500));

        presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Excipient, null);

        var act = () => presentation.RestateIngredient(
            active.Id, IngredientRole.Excipient, null, null);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(IngredientErrors.CompositionNeedsAnActive);
    }

    [Fact]
    public void RestatingCorrectsTheStrengthAndKeepsTheSubstance()
    {
        var presentation = Presentation();
        var substanceId = SubstanceId.New();

        var ingredient = presentation.AddIngredient(
            substanceId, IngredientRole.Active, Mg(500));

        presentation.RestateIngredient(
            ingredient.Id, IngredientRole.Active, Mg(250), null);

        var corrected = presentation.Ingredients.Single();

        corrected.Id.Should().Be(ingredient.Id);
        corrected.SubstanceId.Should().Be(substanceId);
        corrected.Strength!.NumeratorValue.Should().Be(250m);
    }

    /// <summary>
    /// The rule the constructor owns is checked on the way back in, so a
    /// restate cannot reach a state a create could not.
    /// </summary>
    [Fact]
    public void RestatingAnActiveWithoutAStrengthIsRefused()
    {
        var presentation = Presentation();

        var ingredient = presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Active, Mg(500));

        var act = () => presentation.RestateIngredient(
            ingredient.Id, IngredientRole.Active, null, null);

        act.Should().Throw<DomainException>()
            .WithMessage(IngredientErrors.ActiveNeedsAStrength);
    }

    /// <summary>
    /// Swapping a substance is add-then-remove, and because the new active goes
    /// in first the last-active guard never blocks it. This is the flow that
    /// would be a trap if restate could change the substance.
    /// </summary>
    [Fact]
    public void SwappingTheOnlyActiveWorksByAddingBeforeRemoving()
    {
        var presentation = Presentation();

        var wrong = presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Active, Mg(500));

        var right = presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Active, Mg(500));

        presentation.RemoveIngredient(wrong.Id);

        presentation.Ingredients.Single().Id.Should().Be(right.Id);
    }

    [Fact]
    public void RemovingSomethingThatIsNotThereIsNotFound()
    {
        var presentation = Presentation();

        var act = () => presentation.RemoveIngredient(IngredientId.New());

        act.Should().Throw<NotFoundException>()
            .WithMessage(IngredientErrors.NotFound);
    }

    [Fact]
    public void AnIngredientMustNameASubstance()
    {
        var presentation = Presentation();

        var act = () => presentation.AddIngredient(
            null!, IngredientRole.Active, Mg(500));

        act.Should().Throw<DomainException>()
            .WithMessage(IngredientErrors.SubstanceRequired);
    }
}
