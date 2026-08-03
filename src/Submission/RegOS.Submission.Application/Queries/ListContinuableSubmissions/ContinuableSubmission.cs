namespace RegOS.Submission.Application.Queries.ListContinuableSubmissions;

/// <param name="SequenceNumber">
/// What the opener was filed as. This is the value eCTD writes as
/// <c>submission-id</c> for every sequence in the activity, which is why an
/// unpublished submission can never appear in this list.
/// </param>
/// <param name="SubmissionTypeName">
/// What the activity is — "Annual Report", "IND Safety Report". The screen lists
/// activities by this rather than by a bare number, because a filer chooses
/// between activities, not between sequences.
/// </param>
public sealed record ContinuableSubmission(
    Guid Id,
    int SequenceNumber,
    string Title,
    string SubmissionTypeName);
