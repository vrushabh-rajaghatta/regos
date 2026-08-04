namespace RegOS.ReferenceData.Application.Queries.Clinical.GetClinicalVocabulary;

/// <summary>
/// The words a clinical statement may be drawn from.
/// </summary>
/// <remarks>
/// Served over the API so the client offers exactly what the server accepts —
/// and so the <c>system</c> travels, which is how the screen can say these are
/// RegOS's own words rather than MedDRA's.
/// </remarks>
public sealed record GetClinicalVocabularyQuery();
