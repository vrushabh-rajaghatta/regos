namespace RegOS.ReferenceData.Application.Queries.Labels.GetLabelVocabulary;

/// <summary>
/// The kinds of label a company may hold centrally.
/// </summary>
/// <remarks>
/// Served over the API rather than hard-coded in the form, so the client offers
/// exactly what the server accepts. A picker built from a list the frontend
/// keeps its own copy of is the first place the two drift.
/// </remarks>
public sealed record GetLabelVocabularyQuery();
