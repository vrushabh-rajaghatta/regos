namespace RegOS.ReferenceData.Application.Queries.Presentations.GetPharmaceuticalVocabulary;

/// <summary>
/// The dose forms, routes and units of presentation a caller may name.
/// </summary>
/// <remarks>
/// Served over the API rather than hard-coded in the form, so the client offers
/// exactly what the server accepts. A picker built from the frontend's own copy
/// of the list is the first place the two drift — and this vocabulary is
/// expected to be replaced wholesale when licensed terminology arrives.
/// </remarks>
public sealed record GetPharmaceuticalVocabularyQuery();
