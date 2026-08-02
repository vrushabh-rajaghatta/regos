using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Services;

/// <summary>
/// The state of an application's filing history, as at the moment it was read.
/// </summary>
/// <param name="NextSequenceNumber">
/// What to publish under. <c>0</c> for the first sequence in an application —
/// eCTD numbering starts at <c>0000</c>.
/// </param>
/// <param name="PreviousPublishedSequenceNumber">
/// The highest sequence number already published, or null when there is none.
/// Carried alongside the next number rather than left for the caller to derive,
/// so the aggregate's contiguity check compares against a fact someone read from
/// the database instead of arithmetic the caller did twice.
/// </param>
/// <param name="Placements">
/// Every placement that sequence carried. Empty for a first filing — and empty
/// is a different statement from <em>the previous sequence placed nothing</em>,
/// which is why <see cref="PreviousPublishedSequenceNumber"/> is carried
/// separately rather than inferred from this being empty.
/// </param>
public sealed record PublicationBaseline(
    int NextSequenceNumber,
    int? PreviousPublishedSequenceNumber,
    IReadOnlyCollection<PublishedPlacement> Placements);
