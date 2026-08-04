using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.Contraindications;

/// <summary>
/// Who must not be given this product in this market, and why.
/// </summary>
/// <remarks>
/// <b>It has no status history, and that asymmetry with <c>Indication</c> is the
/// design.</b> An indication is an <em>authorisation</em>: an authority grants
/// it, extends it, restricts it, withdraws it, and each of those is a decision
/// it owns. A contraindication is <em>content inside an approved label</em> —
/// nobody files a variation to withdraw contraindication #4; they file a revised
/// summary of product characteristics. Its historical context is therefore the
/// <c>LocalLabelRevision</c> that published it, and giving it a history of its
/// own would invent a lifecycle no regulator operates.
/// <para>
/// The falsifier, recorded so it can be checked rather than assumed: a market
/// withdrawing a single contraindication with no new approved labelling
/// document. See <c>docs/domain-model/labeling.md</c>.
/// </para>
/// <para>
/// Same coded-plus-text split as <c>Indication</c>: the code is what makes
/// <em>"which markets contraindicate X?"</em> a question at all, and the text is
/// what this market's label actually says.
/// </para>
/// </remarks>
public sealed class Contraindication : AggregateRoot<ContraindicationId>
{
    public const int LabelTextMaxLength = 4000;

    private readonly List<Population> _populations = [];

    private Contraindication()
    {
    }

    public TenantId TenantId { get; private set; } = default!;

    public MedicinalProductId MedicinalProductId { get; private set; } = default!;

    public CodedConcept Condition { get; private set; } = default!;

    public string LabelText { get; private set; } = default!;

    public DateTime CreatedOnUtc { get; private set; }

    /// <summary>Who it applies to. Empty means everyone, which is ordinary.</summary>
    public IReadOnlyCollection<Population> Populations
        => _populations.AsReadOnly();

    public static Contraindication Record(
        TenantId tenantId,
        MedicinalProductId medicinalProductId,
        string? conditionCode,
        string? labelText,
        DateTime createdOnUtc)
    {
        if (tenantId is null)
            throw new DomainException(ClinicalStatementErrors.TenantRequired);

        if (medicinalProductId is null)
            throw new DomainException(
                ClinicalStatementErrors.MedicinalProductRequired);

        return new Contraindication
        {
            Id = ContraindicationId.New(),
            TenantId = tenantId,
            MedicinalProductId = medicinalProductId,
            Condition = ClinicalCondition.Resolve(conditionCode),
            LabelText = ClinicalCondition.NormalizeText(
                labelText, LabelTextMaxLength, ContraindicationErrors.LabelTextTooLong),
            CreatedOnUtc = createdOnUtc
        };
    }

    public void RestateLabelText(string? labelText)
        => LabelText = ClinicalCondition.NormalizeText(
            labelText, LabelTextMaxLength, ContraindicationErrors.LabelTextTooLong);

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

    /// <summary>
    /// Corrects a qualifier in place — the second demonstration that
    /// <c>Population</c> having identity is earned rather than assumed.
    /// </summary>
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
}
