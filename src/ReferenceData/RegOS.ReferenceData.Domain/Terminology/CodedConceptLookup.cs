namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// Resolves a code against a fixed vocabulary, case-insensitively.
/// </summary>
/// <remarks>
/// <b>Extracted on the third occurrence, which is what ADR-018 asks for.</b>
/// <see cref="SubstanceVocabulary"/> wrote it, <see cref="PharmaceuticalVocabulary"/>
/// duplicated it and recorded that a third vocabulary was the trigger, and
/// <see cref="MeasurementVocabulary"/> is the third. The two copies were
/// identical by then, so this is a collapse rather than a design.
/// <para>
/// <b>It hands back a fresh instance, never the catalogued one.</b> A resolved
/// concept is about to be persisted as an owned entity, and EF tracks one
/// against exactly one owner — handing out the shared object makes the second
/// entity to use it look like it has no value at all. That is how S001's seed
/// failed the first time it ran, and it is now guarded in one place instead of
/// remembered in three.
/// </para>
/// </remarks>
internal static class CodedConceptLookup
{
    public static CodedConcept? Find(
        IReadOnlyList<CodedConcept> vocabulary, string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var trimmed = code.Trim();

        var match = vocabulary.FirstOrDefault(
            x => string.Equals(x.Code, trimmed, StringComparison.OrdinalIgnoreCase));

        return match is null
            ? null
            : CodedConcept.Create(match.System, match.Code, match.Display);
    }
}
