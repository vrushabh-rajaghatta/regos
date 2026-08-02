using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Submission.Domain.Submission;

/// <summary>
/// The published sequence that opened the regulatory activity a new submission
/// continues — everything about it that the rules need to see.
/// </summary>
/// <remarks>
/// Supplied to <see cref="Submission.Create"/> by the application layer, exactly
/// as <see cref="PublishedPlacement"/> is supplied to
/// <see cref="Submission.Publish"/>: a Submission is a root, so another
/// submission is outside its consistency boundary. <b>The facts come from
/// outside; the rules that read them live in the aggregate</b> (ADR-044
/// decision 6).
/// <para>
/// The three fields beyond the id are not decoration — each is exactly one
/// invariant's evidence, and carrying them here is what lets a domain test reach
/// rules that would otherwise be scattered across handlers.
/// </para>
/// </remarks>
/// <param name="SequenceNumber">
/// What the origin was filed as, or null if it is still a draft. eCTD renders
/// <c>submission-id</c> as this number, so an unpublished origin cannot be
/// pointed at — there is nothing to write.
/// </param>
/// <param name="IsItselfAnOrigin">
/// Whether the origin opens its own activity rather than continuing another's.
/// <para>
/// FDA example #22 carries <c>submission-id="0001"</c> — the number of the
/// sequence that <em>opened</em> the activity, not of the one immediately
/// before it. So a chain would have to be walked transitively at render time,
/// and this makes one unconstructible instead.
/// </para>
/// </param>
public sealed record OriginatingSubmission(
    SubmissionId Id,
    RegulatoryApplicationId ApplicationId,
    int? SequenceNumber,
    bool IsItselfAnOrigin);
