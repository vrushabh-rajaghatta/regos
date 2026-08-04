using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Domain.Aggregates.ClinicalStatements;

/// <summary>
/// The two facts every clinical statement carries: a coded condition, and the
/// wording the approved label uses for it.
/// </summary>
/// <remarks>
/// <b>Two static helpers, not a base class.</b> ADR-059 §4 rules out a shared
/// type <em>across aggregate roots</em>, and inheritance here would be exactly
/// that — it would put a rule added for undesirable effects into the
/// contraindication that never asked for one. These are the resolution steps
/// each root calls for itself, at the third demonstrated need (ADR-018).
/// <para>
/// The split they enforce is the reason the capstone question exists: the code
/// is what makes an authorisation recognisable across markets, and the text is
/// what one market's label says. Free text alone cannot be asked backwards
/// (ADR-058 §1).
/// </para>
/// </remarks>
internal static class ClinicalCondition
{
    internal static CodedConcept Resolve(string? conditionCode)
    {
        if (string.IsNullOrWhiteSpace(conditionCode))
            throw new DomainException(ClinicalStatementErrors.ConditionRequired);

        return ClinicalConditionVocabulary.ConditionOf(conditionCode)
               ?? throw new DomainException(
                   ClinicalStatementErrors.ConditionNotRecognised);
    }

    internal static string NormalizeText(
        string? labelText,
        int maxLength,
        string tooLongError)
    {
        if (string.IsNullOrWhiteSpace(labelText))
            throw new DomainException(ClinicalStatementErrors.LabelTextRequired);

        var trimmed = labelText.Trim();

        return trimmed.Length > maxLength
            ? throw new DomainException(tooLongError)
            : trimmed;
    }
}
