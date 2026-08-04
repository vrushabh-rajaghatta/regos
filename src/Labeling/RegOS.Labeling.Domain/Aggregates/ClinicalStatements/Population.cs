using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Domain.Aggregates.ClinicalStatements;

/// <summary>
/// Who a clinical statement applies to — an age band, a gender, a
/// physiological state, or any combination of them.
/// </summary>
/// <remarks>
/// <b>An entity, not a value object</b> (EPIC-018 D2), and the test is whether
/// it is <em>amended</em> or <em>replaced</em>. Correcting <em>children 2–12</em>
/// to <em>children 2–11</em> is the same qualifier, corrected — see
/// <see cref="Amend"/>. If that ever collapses into remove-and-re-add, this
/// should become a value object while doing so is still cheap.
/// <para>
/// <b>Owned by exactly one clinical statement, and mapped three times.</b> The
/// <em>shape</em> is shared — S004 confirmed the second and third owners need
/// the same fields and the same rules — while each owner keeps its own table,
/// because an owned entity is tracked against exactly one owner (ADR-058).
/// <para>
/// One CLR type rather than three, on ADR-018's third demonstrated need. This is
/// not the shared <em>base type across aggregate roots</em> ADR-059 §4 forbids:
/// nothing here couples <c>Indication</c> to <c>Contraindication</c>, and either
/// may grow a rule the other does not without touching this file — at which
/// point it stops being one shape and should stop being one class.
/// </para>
/// </para>
/// </remarks>
public sealed class Population : Entity<PopulationId>
{
    public const int DescriptionMaxLength = 500;

    // Parameterless: this entity owns value objects, and EF cannot bind an
    // owned type to a constructor parameter. Enforced by
    // AggregateChildArchitectureTests.
    private Population()
    {
    }

    /// <summary>
    /// The lower age bound, inclusive. Null means "from birth" — an ordinary
    /// statement rather than missing data.
    /// </summary>
    public int? AgeLow { get; private set; }

    /// <summary>The upper age bound, inclusive. Null means "and above".</summary>
    public int? AgeHigh { get; private set; }

    /// <summary>
    /// How the bounds are counted. Required whenever either bound is set: "2 to
    /// 12" without a unit is not a statement anyone can act on.
    /// </summary>
    public CodedConcept? AgeUnit { get; private set; }

    public CodedConcept Gender { get; private set; } = default!;

    /// <summary>Pregnancy, renal impairment, and the rest. Null is ordinary.</summary>
    public CodedConcept? PhysiologicalCondition { get; private set; }

    /// <summary>What the label actually says, when a code cannot carry it.</summary>
    public string? Description { get; private set; }

    internal static Population Create(
        int? ageLow,
        int? ageHigh,
        string? ageUnitCode,
        string? genderCode,
        string? physiologicalConditionCode,
        string? description)
    {
        var population = new Population { Id = PopulationId.New() };

        population.Set(
            ageLow, ageHigh, ageUnitCode, genderCode,
            physiologicalConditionCode, description);

        return population;
    }

    /// <summary>
    /// Corrects this qualifier in place.
    /// </summary>
    /// <remarks>
    /// <b>The method that justifies the identity.</b> A paediatric band written
    /// as 2–12 and corrected to 2–11 is the same qualifier on the same
    /// statement; removing it and adding another would say the label once
    /// applied to a population it never applied to.
    /// </remarks>
    internal void Amend(
        int? ageLow,
        int? ageHigh,
        string? ageUnitCode,
        string? genderCode,
        string? physiologicalConditionCode,
        string? description)
        => Set(
            ageLow, ageHigh, ageUnitCode, genderCode,
            physiologicalConditionCode, description);

    private void Set(
        int? ageLow,
        int? ageHigh,
        string? ageUnitCode,
        string? genderCode,
        string? physiologicalConditionCode,
        string? description)
    {
        if (ageLow is < 0 || ageHigh is < 0)
            throw new DomainException(ClinicalStatementErrors.AgeCannotBeNegative);

        if (ageLow is { } low && ageHigh is { } high && low > high)
            throw new DomainException(ClinicalStatementErrors.AgeRangeInverted);

        // A bound with no unit is not a statement anyone can act on, and a unit
        // with no bound says nothing at all.
        var hasBound = ageLow is not null || ageHigh is not null;
        var unit = ClinicalConditionVocabulary.AgeUnitOf(ageUnitCode);

        if (hasBound && unit is null)
            throw new DomainException(ClinicalStatementErrors.AgeUnitRequired);

        if (!hasBound && unit is not null)
            throw new DomainException(ClinicalStatementErrors.AgeUnitWithoutRange);

        if (ageUnitCode is not null && !string.IsNullOrWhiteSpace(ageUnitCode)
            && unit is null)
        {
            throw new DomainException(ClinicalStatementErrors.AgeUnitNotRecognised);
        }

        if (description is not null
            && description.Trim().Length > DescriptionMaxLength)
        {
            throw new DomainException(ClinicalStatementErrors.DescriptionTooLong);
        }

        AgeLow = ageLow;
        AgeHigh = ageHigh;
        AgeUnit = unit;

        Gender = ClinicalConditionVocabulary.GenderOf(genderCode)
                 ?? throw new DomainException(
                     ClinicalStatementErrors.GenderNotRecognised);

        PhysiologicalCondition = string.IsNullOrWhiteSpace(
                physiologicalConditionCode)
            ? null
            : ClinicalConditionVocabulary.PhysiologicalConditionOf(
                  physiologicalConditionCode)
              ?? throw new DomainException(
                  ClinicalStatementErrors.PhysiologicalConditionNotRecognised);

        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }
}
