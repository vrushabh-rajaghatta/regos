using FluentAssertions;

using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.DrugInteractions;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Substances;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Tests;

public sealed class InteractionTests
{
    private static readonly TenantId Tenant = new(Guid.NewGuid());
    private static readonly MedicinalProductId Market = new(Guid.NewGuid());
    private static readonly DateTime Now = new(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc);

    private static DrugInteraction AnInteraction(
        string? interactant = "warfarin",
        SubstanceId? substanceId = null,
        string? severity = "MAJOR")
        => DrugInteraction.Record(
            Tenant,
            Market,
            "DRUG-DRUG",
            "Concomitant use increases the anticoagulant effect.",
            interactant,
            substanceId,
            "Monitor INR and adjust the dose.",
            severity,
            Now);

    // --- the one invariant S005 adds to the context -------------------------

    /// <summary>
    /// <b>The new rule.</b> Every other statement is meaningful alone — a
    /// contraindication with no population applies to everyone. An interaction
    /// with nothing to interact with is not an under-specified statement; it is
    /// not a statement.
    /// </summary>
    [Fact]
    public void AnInteractionIsBornNamingWhatItIsWith()
    {
        var interaction = AnInteraction();

        interaction.Interactants.Should().ContainSingle();
        interaction.Interactants.Single().Description.Should().Be("warfarin");
    }

    [Fact]
    public void AnInteractionWithNothingToInteractWithIsRefused()
    {
        var record = () => AnInteraction(interactant: "  ");

        record.Should().Throw<DomainException>()
            .WithMessage(DrugInteractionErrors.InteractantRequired);
    }

    [Fact]
    public void TheLastInteractantCannotBeRemoved()
    {
        var interaction = AnInteraction();

        var remove = () => interaction.RemoveInteractant(
            interaction.Interactants.Single().Id);

        remove.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(DrugInteractionErrors.LastInteractantCannotBeRemoved);
    }

    [Fact]
    public void AnInteractantIsRemovedOnceAnotherNamesTheInteraction()
    {
        var interaction = AnInteraction();
        var first = interaction.Interactants.Single().Id;

        interaction.AddInteractant("and other coumarin anticoagulants", null);
        interaction.RemoveInteractant(first);

        interaction.Interactants.Should().ContainSingle();
        interaction.Interactants.Single().Description
            .Should().Be("and other coumarin anticoagulants");
    }

    // --- the seam OtherTherapy predicted ------------------------------------

    /// <summary>
    /// The link arrives <b>beside</b> the text, never instead of it — which is
    /// what <c>OtherTherapy</c> said would happen when somebody needed the
    /// question asked backwards.
    /// </summary>
    [Fact]
    public void AnInteractantMayPointAtASubstanceWeKnow()
    {
        var warfarin = SubstanceId.New();

        var interaction = AnInteraction(substanceId: warfarin);

        var interactant = interaction.Interactants.Single();

        interactant.SubstanceId.Should().Be(warfarin);
        interactant.Description.Should().Be("warfarin");
    }

    /// <summary>
    /// And most do not. Grapefruit juice, alcohol and "CYP3A4 inhibitors" are
    /// not substances RegOS knows, and a required link would make the ordinary
    /// case unrecordable.
    /// </summary>
    [Fact]
    public void MostInteractantsAreNotSubstancesWeKnow()
    {
        var interaction = AnInteraction(interactant: "grapefruit juice");

        interaction.Interactants.Single().SubstanceId.Should().BeNull();
    }

    // --- settled patterns, applied a fourth time ----------------------------

    /// <summary>
    /// Like a contraindication and unlike an indication: content inside an
    /// approved label owns no history of its own.
    /// </summary>
    [Fact]
    public void AnInteractionOwnsNoHistory()
    {
        typeof(DrugInteraction).GetProperties()
            .Select(x => x.Name)
            .Should().NotContain("StatusHistory");
    }

    [Fact]
    public void SeverityIsOptionalBecauseManyLabelsDoNotGradeAnInteraction()
    {
        AnInteraction(severity: null).Severity.Should().BeNull();
    }

    [Fact]
    public void AnUnknownSeverityIsRefused()
    {
        var record = () => AnInteraction(severity: "QUITE-BAD");

        record.Should().Throw<DomainException>()
            .WithMessage(DrugInteractionErrors.SeverityNotRecognised);
    }

    [Fact]
    public void TheKindOfInteractionIsCoded()
    {
        AnInteraction().InteractionType.Code.Should().Be("DRUG-DRUG");
    }

    [Fact]
    public void ManagementIsWhatToDoAndIsOptional()
    {
        var interaction = AnInteraction();

        interaction.Management.Should().Be("Monitor INR and adjust the dose.");

        interaction.RecordManagement(null);

        interaction.Management.Should().BeNull();
    }

    /// <summary>The fourth demonstration that a qualifier amends in place.</summary>
    [Fact]
    public void APopulationIsCorrectedInPlaceOnAnInteraction()
    {
        var interaction = AnInteraction();

        var population = interaction.AddPopulation(
            65, null, "YEAR", "ALL", null, "Elderly patients");

        var id = population.Id;

        interaction.AmendPopulation(
            id, 75, null, "YEAR", "ALL", null, "Elderly patients");

        interaction.Populations.Should().ContainSingle();
        interaction.Populations.Single().Id.Should().Be(id);
        interaction.Populations.Single().AgeLow.Should().Be(75);
    }

    [Fact]
    public void TheQualifierRulesAreUnchangedOnTheFourthParent()
    {
        var add = () => AnInteraction()
            .AddPopulation(2, 12, null, "ALL", null, null);

        add.Should().Throw<DomainException>()
            .WithMessage(ClinicalStatementErrors.AgeUnitRequired);
    }

    [Fact]
    public void RestatingTheWordingLeavesTheInteractantsAlone()
    {
        var interaction = AnInteraction();

        interaction.RestateLabelText("Increases the anticoagulant effect.");

        interaction.LabelText
            .Should().Be("Increases the anticoagulant effect.");
        interaction.Interactants.Should().ContainSingle();
    }
}
