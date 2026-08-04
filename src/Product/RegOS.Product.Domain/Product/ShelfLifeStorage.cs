using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// How long a pack keeps, and what has to be true of its storage for that to
/// hold — <em>"36 months. Do not store above 25 °C."</em>
/// </summary>
/// <remarks>
/// <b>One value object rather than two fields, because the period is only true
/// under the conditions.</b> <em>"36 months"</em> on its own is not a fact;
/// <em>"36 months below 25 °C"</em> is. Splitting them would let a pack keep a
/// shelf life whose precondition had been deleted, and nothing would notice.
/// <para>
/// <b>A pack fact, not a presentation fact</b>
/// (<see href="../../../docs/adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md">ADR-061</see>
/// §1's discriminator, used a third time). The same tablets in an alu-alu
/// blister and in an HDPE bottle have different shelf lives, because the
/// container closure system is what the stability data was generated against —
/// which is <c>PackageItem.Material</c> from S002 earning its place.
/// </para>
/// <para>
/// <b>The conclusion, never the study.</b> This type knows <em>36 months</em>.
/// It does not know the stability protocol, the report, or the dossier section
/// that carries them; those are evidence, and evidence lives elsewhere.
/// </para>
/// <para>
/// <b>The period is kept literal.</b> <em>"3 years"</em> is <c>3</c> and
/// <c>YEAR</c>, never <c>36</c> and <c>MONTH</c>. Normalising would be the first
/// unit conversion in RegOS, against the position <see cref="Strength"/> already
/// records — and a shelf life is quoted back on a label in the words it was
/// approved in.
/// </para>
/// </remarks>
public sealed class ShelfLifeStorage : ValueObject
{
    public const int TextMaxLength = 1000;

    private readonly List<CodedConcept> _storageConditions = [];

    private ShelfLifeStorage()
    {
    }

    /// <summary>
    /// Nothing has been said yet about how long this pack keeps.
    /// </summary>
    /// <remarks>
    /// <b>A value, not a null.</b> Every pack carries a statement and this is
    /// the empty one, so a caller never has to guard a navigation — and
    /// <see cref="IsStated"/> answers the question a null would only imply. It
    /// is also what makes the mapping safe: EF materialises a <em>required</em>
    /// owned reference unconditionally, where an optional one whose columns are
    /// all null is read back as null, silently taking
    /// <see cref="StorageConditions"/> with it.
    /// <para>
    /// <b>A new instance each time, not a shared one.</b> An owned instance
    /// belongs to exactly one owner in EF's change tracker, so handing the same
    /// object to two packs would make the second one untrackable. Value
    /// equality means callers cannot tell the difference.
    /// </para>
    /// </remarks>
    public static ShelfLifeStorage NotStated => new();

    /// <summary>How long it keeps — <c>36</c>, against <see cref="Unit"/>.</summary>
    public decimal? Value { get; private init; }

    /// <summary>
    /// The period the value counts, from
    /// <see cref="SupplyVocabulary.ShelfLifePeriods"/>.
    /// </summary>
    public CodedConcept? Unit { get; private init; }

    /// <summary>
    /// What the label says, in the words it was approved in.
    /// </summary>
    /// <remarks>
    /// <b>Approved wording, not a citation.</b> The structured value above is
    /// what a rule reads; this is what a label prints — the same pairing
    /// <c>Strength</c> and a presentation's name already use.
    /// <para>
    /// It is also where an in-use period lives until somebody asks for it
    /// structured: <em>"After first opening: use within 28 days."</em> Stated
    /// here so the field has a contract rather than becoming the bag that
    /// <c>OtherCharacteristics</c> was refused for being.
    /// </para>
    /// </remarks>
    public string? Text { get; private init; }

    /// <summary>
    /// What has to be true of its storage. Several ordinarily apply at once —
    /// <em>"Do not store above 25 °C. Protect from light."</em>
    /// </summary>
    public IReadOnlyCollection<CodedConcept> StorageConditions
        => _storageConditions.AsReadOnly();

    /// <summary>
    /// True once anything at all has been said. False for
    /// <see cref="NotStated"/>.
    /// </summary>
    public bool IsStated
        => Value is not null
           || Text is not null
           || _storageConditions.Count > 0;

    /// <summary>
    /// True when the statement is <em>"no special storage precautions"</em> —
    /// which is a conclusion somebody reached, not a blank.
    /// </summary>
    public bool NeedsNoSpecialPrecautions
        => _storageConditions.Any(
            x => x.Code == SupplyVocabulary.NoSpecialPrecautionsCode);

    public static ShelfLifeStorage Create(
        decimal? value,
        CodedConcept? unit,
        string? text,
        IEnumerable<CodedConcept>? storageConditions)
    {
        // Half a period is refused for the reason half a pack size is: 36 with
        // no unit could be days, months or years, and a unit with no number
        // says nothing at all.
        if (value is not null && unit is null)
            throw new DomainException(ShelfLifeStorageErrors.PeriodUnitRequired);

        if (unit is not null && value is null)
            throw new DomainException(ShelfLifeStorageErrors.PeriodValueRequired);

        if (value is <= 0)
            throw new DomainException(ShelfLifeStorageErrors.PeriodMustBePositive);

        var trimmed = string.IsNullOrWhiteSpace(text) ? null : text.Trim();

        if (trimmed is not null && trimmed.Length > TextMaxLength)
            throw new DomainException(ShelfLifeStorageErrors.TextTooLong);

        var statement = new ShelfLifeStorage
        {
            Value = value,
            Unit = unit,
            Text = trimmed
        };

        foreach (var condition in storageConditions ?? [])
        {
            if (condition is null)
                continue;

            // Compared by value: two CodedConcepts quoting the same code from
            // the same system are the same condition even though they are two
            // objects. The same call RoutesOfAdministration makes.
            if (statement._storageConditions.Contains(condition))
                throw new BusinessRuleViolationException(
                    ShelfLifeStorageErrors.ConditionAlreadyStated);

            statement._storageConditions.Add(condition);
        }

        // "No special precautions" is a conclusion about the whole set, so it
        // cannot sit beside a precaution. Checked after the loop rather than
        // inside it, because the two orders of entry must be refused alike.
        if (statement.NeedsNoSpecialPrecautions
            && statement._storageConditions.Count > 1)
        {
            throw new BusinessRuleViolationException(
                ShelfLifeStorageErrors.NoSpecialPrecautionsStandsAlone);
        }

        return statement;
    }

    /// <remarks>
    /// <b>The conditions are ordered by code first</b>, so that two statements
    /// naming the same precautions are equal however they were entered. Order is
    /// a fact about the form, not about the storage.
    /// </remarks>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return Unit;
        yield return Text;

        foreach (var condition in _storageConditions
            .OrderBy(x => x.Code, StringComparer.Ordinal))
        {
            yield return condition;
        }
    }
}
