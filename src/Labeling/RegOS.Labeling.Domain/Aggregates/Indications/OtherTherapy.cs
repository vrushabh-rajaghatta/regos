using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Domain.Aggregates.Indications;

/// <summary>
/// Another therapy this statement is qualified by — <em>in combination
/// with metformin</em>, <em>after failure of a TNF inhibitor</em>.
/// </summary>
/// <remarks>
/// The therapy itself is free text, deliberately. It may be a substance RegOS
/// knows, a drug class it does not, or a procedure that is not a product at all,
/// and a required <c>SubstanceId</c> would make two of those three
/// unrecordable. When somebody needs <em>"which of our indications name
/// metformin?"</em> asked backwards, that is the conversation ADR-058 §1
/// already worked through — and it will arrive as an optional link beside the
/// text rather than instead of it.
/// </remarks>
public sealed class OtherTherapy : Entity<OtherTherapyId>
{
    public const int TherapyMaxLength = 250;

    private OtherTherapy()
    {
    }

    public CodedConcept Relationship { get; private set; } = default!;

    public string Therapy { get; private set; } = default!;

    internal static OtherTherapy Create(
        string? relationshipCode,
        string? therapy)
    {
        if (string.IsNullOrWhiteSpace(therapy))
            throw new DomainException(IndicationErrors.TherapyRequired);

        var trimmed = therapy.Trim();

        if (trimmed.Length > TherapyMaxLength)
            throw new DomainException(IndicationErrors.TherapyTooLong);

        return new OtherTherapy
        {
            Id = OtherTherapyId.New(),
            Relationship =
                ClinicalConditionVocabulary.TherapyRelationshipOf(
                    relationshipCode)
                ?? throw new DomainException(
                    IndicationErrors.TherapyRelationshipNotRecognised),
            Therapy = trimmed
        };
    }
}
