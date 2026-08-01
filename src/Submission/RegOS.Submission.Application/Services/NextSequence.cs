namespace RegOS.Submission.Application.Services;

/// <summary>
/// What to file as, and what it must follow.
/// </summary>
/// <param name="Number">
/// The number to publish under. <c>0</c> for the first sequence in an
/// application — eCTD numbering starts at <c>0000</c>.
/// </param>
/// <param name="PreviousPublished">
/// The highest sequence number already published in the application, or null
/// when there is none. Carried alongside <paramref name="Number"/> rather than
/// left for the caller to derive, so the aggregate's contiguity check compares
/// against a fact someone read from the database instead of arithmetic the
/// caller did twice.
/// </param>
public sealed record NextSequence(int Number, int? PreviousPublished);
