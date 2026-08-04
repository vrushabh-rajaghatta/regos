using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.UndesirableEffects;

/// <summary>
/// A side effect this product's approved label lists in this market, and how
/// often it occurs.
/// </summary>
/// <remarks>
/// <b>No status history, for the same reason as <c>Contraindication</c>.</b> A
/// new adverse reaction does not become true because someone edited a row; it
/// becomes true because a revised approved label entered into force. The history
/// is the <c>LocalLabelRevision</c>'s.
/// <para>
/// <b><see cref="Frequency"/> is the one thing the three statement types do not
/// share</b>, and it is what S004 was watching for. It is an attribute, not an
/// invariant: nothing branches on it, no rule reads it, and it changes nothing
/// about how a population qualifies the statement. That is why it argues against
/// a shared domain type and not against the shared persistence mapping.
/// </para>
/// </remarks>
public sealed class UndesirableEffect : AggregateRoot<UndesirableEffectId>
{
    public const int LabelTextMaxLength = 4000;

    private readonly List<Population> _populations = [];

    private UndesirableEffect()
    {
    }

    public TenantId TenantId { get; private set; } = default!;

    public MedicinalProductId MedicinalProductId { get; private set; } = default!;

    /// <summary>The effect itself, coded — nausea, headache, anaphylaxis.</summary>
    public CodedConcept Effect { get; private set; } = default!;

    public string LabelText { get; private set; } = default!;

    /// <summary>
    /// Very common through very rare, as the label states it.
    /// </summary>
    /// <remarks>
    /// <b>Recorded, never computed.</b> The bands have numeric thresholds
    /// (≥1/10, ≥1/100 …) and RegOS does not hold the trial data behind them —
    /// deriving one would be asserting a calculation nobody performed. Null is
    /// ordinary: a label may list an effect without a frequency, and
    /// <em>not known</em> is itself a band.
    /// </remarks>
    public CodedConcept? Frequency { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public IReadOnlyCollection<Population> Populations
        => _populations.AsReadOnly();

    public static UndesirableEffect Record(
        TenantId tenantId,
        MedicinalProductId medicinalProductId,
        string? effectCode,
        string? labelText,
        string? frequencyCode,
        DateTime createdOnUtc)
    {
        if (tenantId is null)
            throw new DomainException(ClinicalStatementErrors.TenantRequired);

        if (medicinalProductId is null)
            throw new DomainException(
                ClinicalStatementErrors.MedicinalProductRequired);

        return new UndesirableEffect
        {
            Id = UndesirableEffectId.New(),
            TenantId = tenantId,
            MedicinalProductId = medicinalProductId,
            Effect = ClinicalCondition.Resolve(effectCode),
            LabelText = ClinicalCondition.NormalizeText(
                labelText, LabelTextMaxLength,
                UndesirableEffectErrors.LabelTextTooLong),
            Frequency = ResolveFrequency(frequencyCode),
            CreatedOnUtc = createdOnUtc
        };
    }

    public void RestateLabelText(string? labelText)
        => LabelText = ClinicalCondition.NormalizeText(
            labelText, LabelTextMaxLength,
            UndesirableEffectErrors.LabelTextTooLong);

    /// <summary>Records how often it occurs, or clears the band.</summary>
    public void RecordFrequency(string? frequencyCode)
        => Frequency = ResolveFrequency(frequencyCode);

    public Population AddPopulation(
        int? ageLow,
        int? ageHigh,
        string? ageUnitCode,
        string? genderCode,
        string? physiologicalConditionCode,
        string? description)
    {
        var population = Population.Create(
            ageLow, ageHigh, ageUnitCode, genderCode,
            physiologicalConditionCode, description);

        _populations.Add(population);

        return population;
    }

    /// <summary>The third demonstration that amendment is in place.</summary>
    public void AmendPopulation(
        PopulationId populationId,
        int? ageLow,
        int? ageHigh,
        string? ageUnitCode,
        string? genderCode,
        string? physiologicalConditionCode,
        string? description)
        => PopulationOf(populationId).Amend(
            ageLow, ageHigh, ageUnitCode, genderCode,
            physiologicalConditionCode, description);

    public void RemovePopulation(PopulationId populationId)
        => _populations.Remove(PopulationOf(populationId));

    private Population PopulationOf(PopulationId populationId)
        => _populations.FirstOrDefault(x => x.Id == populationId)
           ?? throw new NotFoundException(
               ClinicalStatementErrors.PopulationNotFound);

    private static CodedConcept? ResolveFrequency(string? frequencyCode)
        => string.IsNullOrWhiteSpace(frequencyCode)
            ? null
            : ClinicalConditionVocabulary.FrequencyOf(frequencyCode)
              ?? throw new DomainException(
                  UndesirableEffectErrors.FrequencyNotRecognised);
}
