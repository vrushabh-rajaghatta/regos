namespace RegOS.Submission.Domain.Submission;

/// <summary>
/// What a study was called at the moment a sequence was filed.
/// </summary>
/// <remarks>
/// The input to the freeze boundary:
/// <code>
/// Study (mutable) → Publication → frozen snapshot → STF XML
/// </code>
/// A study lives in another context (ADR-056), so the aggregate is told rather
/// than reaching for it — the same shape <see cref="PublishedPlacement"/> takes
/// for the previous sequence's dossier.
/// </remarks>
public sealed record PublishedStudy(
    Guid StudyId,
    string Identifier,
    string Title);
