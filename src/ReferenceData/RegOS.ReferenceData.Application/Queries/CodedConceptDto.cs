namespace RegOS.ReferenceData.Application.Queries;

/// <summary>
/// One term from a controlled vocabulary, on the wire.
/// </summary>
/// <remarks>
/// Lives in <c>Queries</c> rather than under one vocabulary's folder because
/// the label vocabulary is the <b>third</b> to need it, and the second had
/// quietly written its own copy under a different name. That is exactly the
/// demonstrated need [ADR-018](../../../../../docs/adr/ADR-018-rule-of-three.md)
/// waits for — retired here because this slice was already in the file, not as
/// a sweep.
/// </remarks>
/// <param name="System">
/// Sent, not hidden. A screen showing <em>Chemical</em> without saying who
/// named it implies an authority RegOS does not have during MVP; carrying the
/// system to the client is what lets the UI label RegOS's own words as its own
/// (ADR-058 §6).
/// </param>
public sealed record CodedConceptDto(
    string System,
    string Code,
    string Display);
