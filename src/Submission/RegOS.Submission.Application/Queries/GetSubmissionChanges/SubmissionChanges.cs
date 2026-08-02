namespace RegOS.Submission.Application.Queries.GetSubmissionChanges;

/// <summary>
/// What a filing did, relative to the sequence before it.
/// </summary>
/// <param name="SequenceNumber">
/// Null while the submission is a draft — nothing has been filed, so there is
/// nothing this changed.
/// </param>
/// <param name="PreviousSequenceNumber">
/// The sequence this was measured against, or null for a first filing.
/// </param>
/// <param name="Changes">
/// Only the placements that did something. **Unchanged documents are absent**,
/// which is the same choice eCTD makes in a backbone: the interesting part of a
/// sequence is what it altered.
/// </param>
/// <param name="UnchangedCount">
/// How many placements were carried forward untouched. Reported as a count
/// rather than as rows, so "and 14 others unchanged" is answerable without
/// making the list mostly noise.
/// </param>
public sealed record SubmissionChanges(
    int? SequenceNumber,
    int? PreviousSequenceNumber,
    IReadOnlyList<SubmissionChange> Changes,
    int UnchangedCount);

/// <param name="Operation">New, Replace or Delete.</param>
/// <param name="ReplacesDocumentVersionNumber">
/// Which version this superseded, for a Replace. Resolved from the placement the
/// filing pointed at, so the view reads "v2 replaced v1" rather than naming an
/// id no-one recognises.
/// </param>
public sealed record SubmissionChange(
    string Operation,
    string DocumentName,
    string DocumentTypeName,
    string SectionLabel,
    int? DocumentVersionNumber,
    int? ReplacesDocumentVersionNumber);
