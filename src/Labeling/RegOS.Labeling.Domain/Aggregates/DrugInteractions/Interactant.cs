using RegOS.ReferenceData.Domain.Substances;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Domain.Aggregates.DrugInteractions;

/// <summary>
/// The other thing in an interaction — another medicine, a food, a condition, a
/// laboratory test.
/// </summary>
/// <remarks>
/// <b>Free text, with an optional link to a substance beside it.</b> That is
/// exactly what <c>OtherTherapy</c> said would arrive when somebody needed the
/// question asked backwards: <em>"an optional link <b>beside</b> the text, never
/// instead of it"</em>.
/// <para>
/// Instead of it would be wrong, because most interactants are not substances
/// RegOS knows. <em>Grapefruit juice</em>, <em>CYP3A4 inhibitors</em>,
/// <em>alcohol</em>, <em>severe renal impairment</em> — a required
/// <see cref="SubstanceId"/> would make the ordinary case unrecordable, and a
/// label that cannot state its own interactions is not a label.
/// </para>
/// <para>
/// When the link <em>is</em> set, <em>"which of our products interact with
/// warfarin?"</em> becomes a join rather than a string match — the same
/// argument ADR-058 §1 made for splitting <c>Substance</c> from
/// <c>Ingredient</c>, and the reason this is a nullable id rather than nothing.
/// </para>
/// </remarks>
public sealed class Interactant : Entity<InteractantId>
{
    public const int DescriptionMaxLength = 250;

    private Interactant()
    {
    }

    /// <summary>What the label calls it. Always present.</summary>
    public string Description { get; private set; } = default!;

    /// <summary>
    /// The substance this names, when RegOS knows it. Null is the ordinary
    /// case, not missing data.
    /// </summary>
    public SubstanceId? SubstanceId { get; private set; }

    internal static Interactant Create(string? description, SubstanceId? substanceId)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(DrugInteractionErrors.InteractantRequired);

        var trimmed = description.Trim();

        if (trimmed.Length > DescriptionMaxLength)
            throw new DomainException(DrugInteractionErrors.InteractantTooLong);

        return new Interactant
        {
            Id = InteractantId.New(),
            Description = trimmed,
            SubstanceId = substanceId
        };
    }
}
