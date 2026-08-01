namespace RegOS.ReferenceData.Application.Queries.Regulatory.ListCorrespondenceTypes;

/// <summary>
/// The active correspondence vocabulary, for the type picker.
/// </summary>
/// <remarks>
/// Parameterless today and still a record, per SC-003: the convention exists
/// because the third parameter is the one that gets appended to a method
/// signature without anyone noticing. Authority scoping is the likely first
/// parameter (ADR-040 decision 6).
/// </remarks>
public sealed record ListCorrespondenceTypesQuery;
