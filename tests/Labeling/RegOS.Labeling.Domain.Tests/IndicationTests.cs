using FluentAssertions;

using RegOS.Labeling.Domain.Aggregates.Indications;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Tests;

public sealed class IndicationTests
{
    private static readonly TenantId Tenant = new(Guid.NewGuid());
    private static readonly MedicinalProductId Market = new(Guid.NewGuid());
    private static readonly DateTime Now = new(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc);

    private static Indication AnIndication(string condition = "T2DM")
        => Indication.Record(
            Tenant,
            Market,
            condition,
            "Treatment of type 2 diabetes mellitus in adults.",
            new DateOnly(2026, 3, 1),
            Now);

    [Fact]
    public void AnIndicationIsBornApprovedWithOneHistoryEntry()
    {
        var indication = AnIndication();

        indication.CurrentStatus.Should().Be(IndicationStatus.Approved);
        indication.StatusHistory.Should().HaveCount(1);
        indication.StatusHistory.Single().OccurredOn
            .Should().Be(new DateOnly(2026, 3, 1));
    }

    /// <summary>
    /// The decision that shaped this aggregate: an indication has a dated
    /// history of regulatory decisions, not a revision history of documents.
    /// </summary>
    [Fact]
    public void TheAggregateHasNoRevisions()
    {
        typeof(Indication).GetProperties()
            .Select(x => x.Name)
            .Should().NotContain("Revisions");
    }

    [Fact]
    public void TheConditionIsCodedSoItCanBeComparedAcrossMarkets()
    {
        var japan = AnIndication();
        var france = AnIndication();

        // Different label text in each market; one clinical concept.
        france.RestateLabelText("Traitement du diabète de type 2 chez l'adulte.");

        japan.Condition.Should().Be(france.Condition);
        japan.LabelText.Should().NotBe(france.LabelText);

        // Fresh instance per owner — the owned-value trap (ADR-059 §7).
        japan.Condition.Should().NotBeSameAs(france.Condition);
    }

    [Fact]
    public void AConditionOutsideTheVocabularyIsRefused()
    {
        var record = () => AnIndication("NOT-A-CONDITION");

        record.Should().Throw<DomainException>()
            .WithMessage(IndicationErrors.ConditionNotRecognised);
    }

    /// <summary>
    /// Wording and authorisation move independently — the whole reason this
    /// aggregate has no revisions.
    /// </summary>
    [Fact]
    public void RestatingTheTextChangesNothingAboutTheAuthorisation()
    {
        var indication = AnIndication();

        indication.RestateLabelText("Treatment of type 2 diabetes mellitus.");

        indication.CurrentStatus.Should().Be(IndicationStatus.Approved);
        indication.StatusHistory.Should().HaveCount(1);
    }

    [Fact]
    public void ADecisionAppendsAnEntryAndMovesTheCurrentStatus()
    {
        var indication = AnIndication();

        indication.RecordDecision(
            IndicationStatus.Restricted,
            new DateOnly(2026, 9, 1),
            "Restricted to second line after a safety review.");

        indication.CurrentStatus.Should().Be(IndicationStatus.Restricted);
        indication.StatusHistory.Should().HaveCount(2);

        // Nothing is overwritten: the original approval is still readable.
        indication.StatusHistory
            .Should().Contain(x => x.Status == IndicationStatus.Approved);
    }

    [Fact]
    public void AnIndicationDoesNotSilentlyBecomeWithdrawn()
    {
        var indication = AnIndication();

        indication.RecordDecision(
            IndicationStatus.Withdrawn, new DateOnly(2027, 1, 15));

        var entry = indication.StatusHistory
            .Single(x => x.Status == IndicationStatus.Withdrawn);

        // It became withdrawn on a date, which is the point of the history.
        entry.OccurredOn.Should().Be(new DateOnly(2027, 1, 15));
    }

    [Fact]
    public void ThereIsNoTransitionTable()
    {
        var indication = AnIndication();

        // Restricted, expanded again, then withdrawn years later. None of that
        // is incoherent, and nothing here pretends otherwise.
        indication.RecordDecision(
            IndicationStatus.Restricted, new DateOnly(2026, 9, 1));
        indication.RecordDecision(
            IndicationStatus.Expanded, new DateOnly(2027, 4, 1));
        indication.RecordDecision(
            IndicationStatus.Withdrawn, new DateOnly(2030, 1, 1));

        indication.StatusHistory.Should().HaveCount(4);
    }

    [Fact]
    public void ADecisionCannotRestateTheOneAlreadyInForce()
    {
        var indication = AnIndication();

        var again = () => indication.RecordDecision(
            IndicationStatus.Approved, new DateOnly(2026, 9, 1));

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(IndicationErrors.AlreadyInStatus(
                IndicationStatus.Approved));
    }

    [Fact]
    public void BusinessTimeMovesForward()
    {
        var indication = AnIndication();

        var backwards = () => indication.RecordDecision(
            IndicationStatus.Restricted, new DateOnly(2026, 1, 1));

        backwards.Should().Throw<DomainException>()
            .WithMessage(IndicationErrors.OccurredOnBeforePreviousEntry);
    }

    // --- populations: the operations D2 will be judged on -------------------

    [Fact]
    public void APopulationQualifiesTheStatement()
    {
        var indication = AnIndication();

        var paediatric = indication.AddPopulation(
            2, 12, "YEAR", "ALL", null, "Paediatric patients");

        indication.Populations.Should().ContainSingle();
        paediatric.AgeLow.Should().Be(2);
        paediatric.AgeHigh.Should().Be(12);
        paediatric.AgeUnit!.Code.Should().Be("YEAR");
    }

    /// <summary>
    /// <b>The operation that justifies <c>Population</c> having identity.</b>
    /// A band written as 2–12 and corrected to 2–11 is the same qualifier on the
    /// same statement; remove-and-re-add would say the label once applied to a
    /// population it never applied to.
    /// </summary>
    [Fact]
    public void APopulationIsCorrectedInPlaceAndKeepsItsIdentity()
    {
        var indication = AnIndication();

        var paediatric = indication.AddPopulation(
            2, 12, "YEAR", "ALL", null, "Paediatric patients");

        var id = paediatric.Id;

        indication.AmendPopulation(
            id, 2, 11, "YEAR", "ALL", null, "Paediatric patients");

        indication.Populations.Should().ContainSingle();
        indication.Populations.Single().Id.Should().Be(id);
        indication.Populations.Single().AgeHigh.Should().Be(11);
    }

    [Fact]
    public void APopulationRecordedInErrorIsRemoved()
    {
        var indication = AnIndication();

        var population = indication.AddPopulation(
            null, null, null, "MALE", null, null);

        indication.RemovePopulation(population.Id);

        indication.Populations.Should().BeEmpty();
    }

    [Fact]
    public void APopulationFromAnotherIndicationIsNotFound()
    {
        var indication = AnIndication();

        var amend = () => indication.AmendPopulation(
            PopulationId.New(), 1, 2, "YEAR", "ALL", null, null);

        amend.Should().Throw<NotFoundException>()
            .WithMessage(IndicationErrors.PopulationNotFound);
    }

    [Fact]
    public void SeveralPopulationsQualifyOneStatement()
    {
        var indication = AnIndication();

        indication.AddPopulation(18, null, "YEAR", "ALL", null, null);
        indication.AddPopulation(null, null, null, "FEMALE", "PREGNANCY", null);

        indication.Populations.Should().HaveCount(2);
    }

    [Fact]
    public void AnAgeWithNoUnitIsRefused()
    {
        var indication = AnIndication();

        var add = () => indication.AddPopulation(2, 12, null, "ALL", null, null);

        add.Should().Throw<DomainException>()
            .WithMessage(IndicationErrors.AgeUnitRequired);
    }

    [Fact]
    public void AUnitWithNoAgeIsRefused()
    {
        var indication = AnIndication();

        var add = () => indication.AddPopulation(
            null, null, "YEAR", "ALL", null, null);

        add.Should().Throw<DomainException>()
            .WithMessage(IndicationErrors.AgeUnitWithoutRange);
    }

    [Fact]
    public void AnInvertedAgeRangeIsRefused()
    {
        var indication = AnIndication();

        var add = () => indication.AddPopulation(
            12, 2, "YEAR", "ALL", null, null);

        add.Should().Throw<DomainException>()
            .WithMessage(IndicationErrors.AgeRangeInverted);
    }

    [Fact]
    public void NoPopulationMeansEveryone()
    {
        // Empty is ordinary, not incomplete — an indication with no qualifier
        // applies to whoever the label otherwise describes.
        AnIndication().Populations.Should().BeEmpty();
    }

    // --- other therapies ----------------------------------------------------

    [Fact]
    public void AnIndicationMayBeQualifiedByAnotherTherapy()
    {
        var indication = AnIndication();

        var therapy = indication.AddOtherTherapy("COMBINATION", " metformin ");

        therapy.Therapy.Should().Be("metformin");
        therapy.Relationship.Code.Should().Be("COMBINATION");
    }

    [Fact]
    public void TheOtherTherapyIsFreeTextSoADrugClassCanBeNamed()
    {
        var indication = AnIndication();

        // Not a substance RegOS knows, and that is the point.
        var therapy = indication.AddOtherTherapy(
            "AFTER-FAILURE", "a TNF inhibitor");

        therapy.Therapy.Should().Be("a TNF inhibitor");
    }

    [Fact]
    public void AnUnknownRelationshipIsRefused()
    {
        var indication = AnIndication();

        var add = () => indication.AddOtherTherapy("SOMETHING", "metformin");

        add.Should().Throw<DomainException>()
            .WithMessage(IndicationErrors.TherapyRelationshipNotRecognised);
    }

    [Fact]
    public void ATherapyRecordedInErrorIsRemoved()
    {
        var indication = AnIndication();

        var therapy = indication.AddOtherTherapy("COMBINATION", "metformin");

        indication.RemoveOtherTherapy(therapy.Id);

        indication.OtherTherapies.Should().BeEmpty();
    }
}
