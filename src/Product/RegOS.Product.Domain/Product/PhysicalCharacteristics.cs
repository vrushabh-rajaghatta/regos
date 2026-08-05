using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// What the medicine looks like — <em>"White to off-white, round, biconvex
/// film-coated tablet debossed with 10 on one side."</em> Screen word:
/// <b>Appearance</b>.
/// </summary>
/// <remarks>
/// <b>On the presentation, not the pack</b>
/// (<see href="../../../docs/adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md">ADR-061</see>
/// §1's discriminator, and the one place it points the other way). <em>Does it
/// change when the same medicine is sold in a different pack size?</em> A tablet
/// looks identical in a carton of 30 and a carton of 100, so appearance is part
/// of what the medicine <b>is</b> — which is exactly what a presentation
/// records.
/// <para>
/// <b>The conclusion, never the evidence.</b> This says the tablet is white and
/// round. It does not know the specification that fixed it, the batch it was
/// measured on, or the dossier section that argues for it.
/// </para>
/// <para>
/// <b>Small on purpose.</b> Colour, shape, what is stamped on it, and the
/// label's own sentence. No image, no palette, no imprint library — the question
/// is <em>"does this look like the approved medicine?"</em>, not <em>"render a
/// tablet"</em>.
/// </para>
/// </remarks>
public sealed class PhysicalCharacteristics : ValueObject
{
    public const int ImprintMaxLength = 100;
    public const int DescriptionMaxLength = 1000;

    private readonly List<CodedConcept> _colours = [];

    private PhysicalCharacteristics()
    {
    }

    /// <summary>
    /// Nothing has been said yet about what this presentation looks like.
    /// </summary>
    /// <remarks>
    /// <b>The second use of the shape <see cref="ShelfLifeStorage"/> introduced,
    /// and for the identical reason</b> (ADR-018: duplicate on the second,
    /// evaluate on the third). This type also carries an owned collection beside
    /// nullable scalars, so a presentation whose only statement is <em>"white"</em>
    /// has every shared column null — and an <em>optional</em> owned reference is
    /// read back as null in that case, taking <see cref="Colours"/> with it. A
    /// required one is materialised unconditionally.
    /// <para>
    /// A new instance each time: an owned instance belongs to exactly one owner
    /// in EF's change tracker.
    /// </para>
    /// </remarks>
    public static PhysicalCharacteristics NotStated => new();

    /// <summary>
    /// What colour it is. <b>Several is ordinary</b> — a capsule with a white
    /// body and a blue cap is two colours, not one called "white and blue".
    /// </summary>
    public IReadOnlyCollection<CodedConcept> Colours => _colours.AsReadOnly();

    /// <summary>Round, oval, capsule-shaped. One, or none stated.</summary>
    public CodedConcept? Shape { get; private init; }

    /// <summary>
    /// What is stamped, debossed or printed on it — <em>"AZ 10"</em>.
    /// </summary>
    /// <remarks>
    /// <b>Its own field rather than a phrase in
    /// <see cref="Description"/></b>, because it is the one part of an
    /// appearance anybody looks a medicine <em>up</em> by. A poison centre with
    /// a loose tablet has the imprint and nothing else.
    /// </remarks>
    public string? Imprint { get; private init; }

    /// <summary>
    /// The label's own sentence, in the words it was approved in.
    /// </summary>
    /// <remarks>
    /// <b>The third time this pairing appears</b> — a structured fact beside the
    /// approved wording, after <c>Strength</c> with a presentation's name and
    /// <see cref="ShelfLifeStorage"/> with its text. Recorded as an observed
    /// pattern and deliberately <b>not</b> abstracted: the three differ in what
    /// the structured half is, and a shared "coded value plus its wording" type
    /// would name the similarity while hiding every difference.
    /// </remarks>
    public string? Description { get; private init; }

    /// <summary>True once anything at all has been said.</summary>
    public bool IsStated
        => _colours.Count > 0
           || Shape is not null
           || Imprint is not null
           || Description is not null;

    public static PhysicalCharacteristics Create(
        IEnumerable<CodedConcept>? colours,
        CodedConcept? shape,
        string? imprint,
        string? description)
    {
        var trimmedImprint = Trimmed(
            imprint, ImprintMaxLength, PhysicalCharacteristicsErrors.ImprintTooLong);

        var trimmedDescription = Trimmed(
            description,
            DescriptionMaxLength,
            PhysicalCharacteristicsErrors.DescriptionTooLong);

        var appearance = new PhysicalCharacteristics
        {
            Shape = shape,
            Imprint = trimmedImprint,
            Description = trimmedDescription
        };

        foreach (var colour in colours ?? [])
        {
            if (colour is null)
                continue;

            // Compared by value, the same call RoutesOfAdministration and
            // ShelfLifeStorage both make.
            if (appearance._colours.Contains(colour))
                throw new BusinessRuleViolationException(
                    PhysicalCharacteristicsErrors.ColourAlreadyStated);

            appearance._colours.Add(colour);
        }

        return appearance;
    }

    private static string? Trimmed(string? value, int maxLength, string tooLong)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
            throw new DomainException(tooLong);

        return trimmed;
    }

    /// <remarks>
    /// Colours are ordered by code so that two appearances naming the same ones
    /// are equal however they were entered.
    /// </remarks>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Shape;
        yield return Imprint;
        yield return Description;

        foreach (var colour in _colours.OrderBy(x => x.Code, StringComparer.Ordinal))
            yield return colour;
    }
}
