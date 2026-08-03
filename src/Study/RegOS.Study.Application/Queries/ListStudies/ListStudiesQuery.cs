namespace RegOS.Study.Application.Queries.ListStudies;

/// <summary>
/// "What studies do we have?" — across both kinds.
/// </summary>
/// <remarks>
/// Carries no parameters yet, and is a record rather than a bare method
/// signature so that the first one to arrive (a kind filter, when S002 needs to
/// offer only nonclinical studies for a 4.2.x placement) is a field on a named
/// question instead of a parameter appended to a handler (SC-003).
/// </remarks>
public sealed record ListStudiesQuery();
