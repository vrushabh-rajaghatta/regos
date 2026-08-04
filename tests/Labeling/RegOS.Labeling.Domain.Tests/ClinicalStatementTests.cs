using FluentAssertions;

using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.Contraindications;
using RegOS.Labeling.Domain.Aggregates.Indications;
using RegOS.Labeling.Domain.Aggregates.UndesirableEffects;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Tests;

/// <summary>
/// What the three statement types share, and — more importantly — what they do
/// not.
/// </summary>
public sealed class ClinicalStatementTests
{
    private static readonly TenantId Tenant = new(Guid.NewGuid());
    private static readonly MedicinalProductId Market = new(Guid.NewGuid());
    private static readonly DateTime Now = new(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc);

    private static Contraindication AContraindication()
        => Contraindication.Record(
            Tenant,
            Market,
            "HYPERSENS-AS",
            "Hypersensitivity to the active substance.",
            Now);

    private static UndesirableEffect AnUndesirableEffect(
        string? frequency = "COMMON")
        => UndesirableEffect.Record(
            Tenant, Market, "NAUSEA", "Nausea.", frequency, Now);

    // --- the asymmetry that is the design ----------------------------------

    /// <summary>
    /// <b>The S004 decision, asserted rather than commented.</b> An indication is
    /// an authorisation the authority acts on directly, so it owns the history
    /// of those decisions. A contraindication and an undesirable effect are
    /// content inside an approved label — nobody files a variation to withdraw
    /// contraindication #4, they file a revised SmPC — so their history is the
    /// <c>LocalLabelRevision</c> that published them.
    /// </summary>
    [Fact]
    public void OnlyTheAuthorisationOwnsAHistory()
    {
        Names<Indication>().Should().Contain("StatusHistory");

        Names<Contraindication>().Should().NotContain("StatusHistory");
        Names<UndesirableEffect>().Should().NotContain("StatusHistory");

        static IEnumerable<string> Names<T>()
            => typeof(T).GetProperties().Select(x => x.Name);
    }

    /// <summary>
    /// The one attribute the three do not share, and the thing S004 was watching
    /// for. It is an attribute, not an invariant — nothing branches on it.
    /// </summary>
    [Fact]
    public void OnlyTheUndesirableEffectCarriesAFrequency()
    {
        AnUndesirableEffect().Frequency!.Code.Should().Be("COMMON");

        typeof(Contraindication).GetProperties()
            .Select(x => x.Name)
            .Should().NotContain("Frequency");
    }

    [Fact]
    public void AFrequencyIsOptionalBecauseALabelMayNotStateOne()
    {
        AnUndesirableEffect(frequency: null).Frequency.Should().BeNull();
    }

    [Fact]
    public void AnUnknownFrequencyIsRefused()
    {
        var record = () => AnUndesirableEffect("SOMETIMES");

        record.Should().Throw<DomainException>()
            .WithMessage(UndesirableEffectErrors.FrequencyNotRecognised);
    }

    /// <summary>
    /// The frequency bands are recorded, never computed: their thresholds rest
    /// on trial data RegOS does not hold.
    /// </summary>
    [Fact]
    public void NotKnownIsItselfABand()
    {
        AnUndesirableEffect("NOT-KNOWN").Frequency!.Display
            .Should().Be("Not known");
    }

    // --- what they do share -------------------------------------------------

    /// <summary>
    /// The second and third demonstrations that <c>Population</c> having
    /// identity is earned: a band corrected from 12+ to 6+ is the same
    /// qualifier, on both parents.
    /// </summary>
    [Fact]
    public void APopulationIsCorrectedInPlaceOnAContraindication()
    {
        var statement = AContraindication();

        var population = statement.AddPopulation(
            12, null, "YEAR", "ALL", null, null);

        var id = population.Id;

        statement.AmendPopulation(id, 6, null, "YEAR", "ALL", null, null);

        statement.Populations.Should().ContainSingle();
        statement.Populations.Single().Id.Should().Be(id);
        statement.Populations.Single().AgeLow.Should().Be(6);
    }

    [Fact]
    public void APopulationIsCorrectedInPlaceOnAnUndesirableEffect()
    {
        var statement = AnUndesirableEffect();

        var population = statement.AddPopulation(
            null, null, null, "FEMALE", "PREGNANCY", null);

        var id = population.Id;

        statement.AmendPopulation(
            id, null, null, null, "FEMALE", "LACTATION", null);

        statement.Populations.Should().ContainSingle();
        statement.Populations.Single().Id.Should().Be(id);
        statement.Populations.Single().PhysiologicalCondition!.Code
            .Should().Be("LACTATION");
    }

    /// <summary>
    /// The qualifier rules are identical on all three parents — which is what
    /// made the shared persistence mapping honest rather than convenient.
    /// </summary>
    [Fact]
    public void TheQualifierRulesAreTheSameWhicheverStatementOwnsIt()
    {
        var contraindication = () => AContraindication()
            .AddPopulation(2, 12, null, "ALL", null, null);

        var effect = () => AnUndesirableEffect()
            .AddPopulation(2, 12, null, "ALL", null, null);

        contraindication.Should().Throw<DomainException>()
            .WithMessage(ClinicalStatementErrors.AgeUnitRequired);

        effect.Should().Throw<DomainException>()
            .WithMessage(ClinicalStatementErrors.AgeUnitRequired);
    }

    [Fact]
    public void EachStatementOwnsItsOwnPopulations()
    {
        var contraindication = AContraindication();
        var effect = AnUndesirableEffect();

        var population = contraindication.AddPopulation(
            18, null, "YEAR", "ALL", null, null);

        // A qualifier belongs to exactly one statement — the ownership ADR-058
        // paid for, and the reason the three keep separate tables.
        var amend = () => effect.AmendPopulation(
            population.Id, 21, null, "YEAR", "ALL", null, null);

        amend.Should().Throw<NotFoundException>()
            .WithMessage(ClinicalStatementErrors.PopulationNotFound);
    }

    [Fact]
    public void TheConditionIsCodedOnEveryStatementType()
    {
        AContraindication().Condition.Code.Should().Be("HYPERSENS-AS");
        AnUndesirableEffect().Effect.Code.Should().Be("NAUSEA");
    }

    [Fact]
    public void AConditionOutsideTheVocabularyIsRefusedOnEveryType()
    {
        var contraindication = () => Contraindication.Record(
            Tenant, Market, "NOPE", "text", Now);

        var effect = () => UndesirableEffect.Record(
            Tenant, Market, "NOPE", "text", null, Now);

        contraindication.Should().Throw<DomainException>()
            .WithMessage(ClinicalStatementErrors.ConditionNotRecognised);

        effect.Should().Throw<DomainException>()
            .WithMessage(ClinicalStatementErrors.ConditionNotRecognised);
    }

    /// <summary>
    /// The commonest contraindication in pharmaceutical labelling. S004 added it
    /// because the aggregate would otherwise have looked usable without being
    /// able to express the ordinary case.
    /// </summary>
    [Fact]
    public void HypersensitivityToTheActiveSubstanceIsExpressible()
    {
        AContraindication().LabelText
            .Should().Be("Hypersensitivity to the active substance.");
    }

    [Fact]
    public void RestatingTheWordingLeavesTheStatementOtherwiseUntouched()
    {
        var statement = AContraindication();

        statement.AddPopulation(18, null, "YEAR", "ALL", null, null);
        statement.RestateLabelText("Known hypersensitivity to the active substance.");

        statement.LabelText
            .Should().Be("Known hypersensitivity to the active substance.");
        statement.Populations.Should().ContainSingle();
    }
}
