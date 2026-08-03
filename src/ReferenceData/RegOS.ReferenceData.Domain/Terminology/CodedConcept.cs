using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// A term drawn from a controlled vocabulary — <em>which</em> vocabulary, the
/// code within it, and the words a human reads.
/// </summary>
/// <remarks>
/// <b><see cref="System"/> is the whole point of this type</b> (ADR-058 §3).
/// RegOS ships a curated internal vocabulary during MVP and expects to replace
/// it with licensed terminology later; carrying the naming authority on every
/// coded value from day one is what makes that replacement a data migration
/// rather than a redesign. A seeded row is
/// <c>("regos-internal", "CHEMICAL", "Chemical")</c>; the same field later holds
/// <c>("edqm", "10219000", "Tablet")</c>.
/// <para>
/// This is why the type exists at all rather than a bare string per field. A
/// column holding <c>"Tablet"</c> cannot say whether that word is ours or
/// EDQM's, and every consumer downstream has to guess.
/// </para>
/// <para>
/// <b>It lives in <c>ReferenceData</c> and not <c>SharedKernel</c></b>
/// ([ADR-017](../../../../docs/adr/ADR-017-shared-kernel-scope.md)): controlled
/// regulatory terminology is important and is not a primitive with no domain
/// meaning. It lives here rather than in <c>Product</c> because
/// <c>Substance</c> carries two of them, and <c>Product</c> as its home would
/// invert the dependency <c>Product → ReferenceData</c>.
/// </para>
/// </remarks>
public sealed class CodedConcept : ValueObject
{
    public const int SystemMaxLength = 50;
    public const int CodeMaxLength = 100;
    public const int DisplayMaxLength = 250;

    private CodedConcept()
    {
    }

    /// <summary>Who named it — <c>regos-internal</c> during MVP.</summary>
    public string System { get; private init; } = default!;

    /// <summary>The code within that system.</summary>
    public string Code { get; private init; } = default!;

    /// <summary>What a human reads.</summary>
    public string Display { get; private init; } = default!;

    /// <summary>
    /// True while this term is RegOS's own word rather than a licensed one.
    /// The flag a screen uses to say so out loud instead of implying authority
    /// the platform does not have.
    /// </summary>
    public bool IsInternal => System == CodingSystems.RegosInternal;

    public static CodedConcept Create(string system, string code, string display)
    {
        return new CodedConcept
        {
            System = Required(
                system, SystemMaxLength,
                CodedConceptErrors.SystemRequired,
                CodedConceptErrors.SystemTooLong),

            Code = Required(
                code, CodeMaxLength,
                CodedConceptErrors.CodeRequired,
                CodedConceptErrors.CodeTooLong),

            Display = Required(
                display, DisplayMaxLength,
                CodedConceptErrors.DisplayRequired,
                CodedConceptErrors.DisplayTooLong)
        };
    }

    /// <summary>
    /// A term from RegOS's own vocabulary. Every seeded concept in the platform
    /// is created this way, so that <c>regos-internal</c> is written by the
    /// factory rather than remembered at each call site.
    /// </summary>
    public static CodedConcept Internal(string code, string display)
        => Create(CodingSystems.RegosInternal, code, display);

    /// <remarks>
    /// <b><see cref="Display"/> is not an equality component.</b> Two values
    /// quoting the same code from the same system are the same concept even if
    /// one's label has since been re-worded — which is what lets a vocabulary's
    /// wording be corrected without every stored value ceasing to match.
    /// </remarks>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return System;
        yield return Code;
    }

    public override string ToString() => $"{System}|{Code}";

    private static string Required(
        string value, int maxLength, string requiredError, string tooLongError)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(requiredError);

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
            throw new DomainException(tooLongError);

        return trimmed;
    }
}
