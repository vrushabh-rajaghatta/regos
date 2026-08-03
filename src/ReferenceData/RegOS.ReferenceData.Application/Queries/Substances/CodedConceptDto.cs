namespace RegOS.ReferenceData.Application.Queries.Substances;

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
