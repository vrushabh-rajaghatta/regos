using RegOS.Process.Domain.Aggregates.ProcessObjectives;

namespace RegOS.Process.Application.Queries.ListObjectivePlans;

/// <summary>
/// Every attempt at one objective. <b>Plural on purpose</b> — an objective may
/// be attempted more than once (ADR-065 decision 3), and a withdrawn filing
/// re-attempted later is the case that makes it true.
/// </summary>
public sealed record ListObjectivePlansQuery(ProcessObjectiveId ProcessObjectiveId);
