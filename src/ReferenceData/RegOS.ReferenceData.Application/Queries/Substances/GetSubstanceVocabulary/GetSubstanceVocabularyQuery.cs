namespace RegOS.ReferenceData.Application.Queries.Substances.GetSubstanceVocabulary;

/// <summary>
/// The words a substance's class and type may be drawn from.
/// </summary>
/// <remarks>
/// Served over the API rather than hard-coded in the form, so the client offers
/// exactly what the server accepts. A picker built from a list the frontend
/// keeps its own copy of is the first place the two drift.
/// </remarks>
public sealed record GetSubstanceVocabularyQuery();
