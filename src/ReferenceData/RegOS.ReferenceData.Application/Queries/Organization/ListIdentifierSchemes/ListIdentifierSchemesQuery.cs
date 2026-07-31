namespace RegOS.ReferenceData.Application.Queries.Organization.ListIdentifierSchemes;

/// <summary>
/// "Which registries issue identifiers?" — the whole list, unfiltered.
/// </summary>
/// <remarks>
/// Parameterless today, and still a record rather than a bare
/// <c>HandleAsync()</c>: SC-003 exists because a query's shape otherwise lives
/// in a method signature, where the first filter gets appended without anyone
/// noticing. The sibling reference-data queries predate the rule and are
/// carried on the grandfathered list; new ones do not join it.
/// </remarks>
public sealed record ListIdentifierSchemesQuery;
