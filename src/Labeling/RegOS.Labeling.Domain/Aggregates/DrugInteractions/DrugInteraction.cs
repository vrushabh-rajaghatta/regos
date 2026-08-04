using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Substances;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Aggregates.DrugInteractions;

/// <summary>
/// What this product clashes with in this market, and what to do about it.
/// </summary>
/// <remarks>
/// <b>Named <c>DrugInteraction</c>, not RIM's <c>Interaction</c>.</b>
/// <c>RegOS.Interaction</c> is already a bounded context — the health
/// authority's letters, questions and meetings (ADR-040) — so the bare noun
/// forces an alias in every file that sees both, which is the collision this
/// epic renamed <c>Labeling</c> to <c>LocalLabel</c> to avoid. Second mechanical
/// departure from RIM's noun, same reason. The screen still says
/// <b>Interactions</b>.
/// </remarks>
/// <remarks>
/// <b>The fourth clinical statement, and it applies settled patterns.</b> Coded
/// classification, the label's own wording, owned populations, and — like
/// <c>Contraindication</c> and <c>UndesirableEffect</c> and unlike
/// <c>Indication</c> — <b>no history of its own</b>: an interaction is content
/// inside an approved label, so what changes it is a new
/// <c>LocalLabelRevision</c>.
/// <para>
/// <b>One invariant here is new to the context: an interaction must name at
/// least one <see cref="Interactant"/>.</b> Every other statement is meaningful
/// alone — a contraindication with no population applies to everyone, an
/// indication with no therapy is simply unqualified. An interaction with nothing
/// to interact with is not an under-specified statement; it is not a statement.
/// That is why the interactant is supplied to <see cref="Record"/> rather than
/// added afterwards, and why <see cref="RemoveInteractant"/> refuses to remove
/// the last one.
/// </para>
/// </remarks>
public sealed class DrugInteraction : AggregateRoot<DrugInteractionId>
{
    public const int LabelTextMaxLength = 4000;
    public const int ManagementMaxLength = 2000;

    private readonly List<Interactant> _interactants = [];
    private readonly List<Population> _populations = [];

    private DrugInteraction()
    {
    }

    public TenantId TenantId { get; private set; } = default!;

    public MedicinalProductId MedicinalProductId { get; private set; } = default!;

    /// <summary>With another medicine, with food, with a condition, with a test.</summary>
    public CodedConcept InteractionType { get; private set; } = default!;

    /// <summary>What happens, in the approved label's words.</summary>
    public string LabelText { get; private set; } = default!;

    /// <summary>
    /// What to do about it — monitor, reduce the dose, avoid. Null is ordinary:
    /// a label may describe an interaction without prescribing an action.
    /// </summary>
    public string? Management { get; private set; }

    /// <summary>
    /// How much it matters clinically. Nullable, because many labels describe an
    /// interaction without grading it, and inventing a grade would assert a
    /// clinical judgement nobody made.
    /// </summary>
    public CodedConcept? Severity { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    /// <summary>What it interacts with. Never empty.</summary>
    public IReadOnlyCollection<Interactant> Interactants
        => _interactants.AsReadOnly();

    public IReadOnlyCollection<Population> Populations
        => _populations.AsReadOnly();

    /// <param name="interactant">
    /// Supplied here rather than added afterwards, because an interaction with
    /// nothing to interact with is not a statement at all.
    /// </param>
    public static DrugInteraction Record(
        TenantId tenantId,
        MedicinalProductId medicinalProductId,
        string? interactionTypeCode,
        string? labelText,
        string? interactant,
        SubstanceId? interactantSubstanceId,
        string? management,
        string? severityCode,
        DateTime createdOnUtc)
    {
        if (tenantId is null)
            throw new DomainException(ClinicalStatementErrors.TenantRequired);

        if (medicinalProductId is null)
            throw new DomainException(
                ClinicalStatementErrors.MedicinalProductRequired);

        var interaction = new DrugInteraction
        {
            Id = DrugInteractionId.New(),
            TenantId = tenantId,
            MedicinalProductId = medicinalProductId,
            InteractionType = ResolveType(interactionTypeCode),
            LabelText = ClinicalCondition.NormalizeText(
                labelText, LabelTextMaxLength,
                DrugInteractionErrors.LabelTextTooLong),
            Severity = ResolveSeverity(severityCode),
            CreatedOnUtc = createdOnUtc
        };

        interaction.SetManagement(management);

        interaction._interactants.Add(
            Interactant.Create(interactant, interactantSubstanceId));

        return interaction;
    }

    public void RestateLabelText(string? labelText)
        => LabelText = ClinicalCondition.NormalizeText(
            labelText, LabelTextMaxLength, DrugInteractionErrors.LabelTextTooLong);

    public void RecordManagement(string? management) => SetManagement(management);

    public void RecordSeverity(string? severityCode)
        => Severity = ResolveSeverity(severityCode);

    /// <summary>
    /// Names another thing this interaction is with — <em>and other CYP3A4
    /// inhibitors</em>.
    /// </summary>
    public Interactant AddInteractant(
        string? description,
        SubstanceId? substanceId)
    {
        var interactant = Interactant.Create(description, substanceId);

        _interactants.Add(interactant);

        return interactant;
    }

    /// <remarks>
    /// <b>Refuses to remove the last one.</b> The alternative — allowing an
    /// interaction to exist with nothing to interact with — would make the
    /// aggregate representable in a state no label can express, and would leave
    /// the repair to whoever noticed.
    /// </remarks>
    public void RemoveInteractant(InteractantId interactantId)
    {
        var interactant = _interactants
            .FirstOrDefault(x => x.Id == interactantId)
            ?? throw new NotFoundException(
                DrugInteractionErrors.InteractantNotFound);

        if (_interactants.Count == 1)
            throw new BusinessRuleViolationException(
                DrugInteractionErrors.LastInteractantCannotBeRemoved);

        _interactants.Remove(interactant);
    }

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

    /// <summary>The fourth demonstration that amendment is in place.</summary>
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

    private void SetManagement(string? management)
    {
        if (management is not null
            && management.Trim().Length > ManagementMaxLength)
        {
            throw new DomainException(DrugInteractionErrors.ManagementTooLong);
        }

        Management = string.IsNullOrWhiteSpace(management)
            ? null
            : management.Trim();
    }

    private Population PopulationOf(PopulationId populationId)
        => _populations.FirstOrDefault(x => x.Id == populationId)
           ?? throw new NotFoundException(
               ClinicalStatementErrors.PopulationNotFound);

    private static CodedConcept ResolveType(string? interactionTypeCode)
    {
        if (string.IsNullOrWhiteSpace(interactionTypeCode))
            throw new DomainException(DrugInteractionErrors.TypeRequired);

        return ClinicalConditionVocabulary.InteractionTypeOf(interactionTypeCode)
               ?? throw new DomainException(
                   DrugInteractionErrors.TypeNotRecognised);
    }

    private static CodedConcept? ResolveSeverity(string? severityCode)
        => string.IsNullOrWhiteSpace(severityCode)
            ? null
            : ClinicalConditionVocabulary.InteractionSeverityOf(severityCode)
              ?? throw new DomainException(
                  DrugInteractionErrors.SeverityNotRecognised);
}
