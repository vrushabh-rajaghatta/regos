using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Submission.Application.Services;

/// <summary>
/// Answers what a submission should be filed as, for an application whose other
/// sequences the aggregate cannot see.
/// </summary>
/// <remarks>
/// A <c>Submission</c> is a root, so its siblings sit outside its consistency
/// boundary and the numbering authority necessarily lives here rather than in
/// the aggregate (ADR-044 decision 6). The two existing numbering precedents —
/// <c>ProductDocument</c> and <c>RegulatoryTemplate</c> — both number an
/// <em>owned collection</em> from inside the root, which is why neither
/// transfers.
/// <para>
/// <b>Nothing is reserved.</b> The name says <em>get</em> for that reason: the
/// answer is true at the moment it is read and can be taken by a concurrent
/// publish before the caller uses it. The unique index on (application,
/// sequence number) is what makes that safe, and the caller retries.
/// </para>
/// <para>
/// The sixth policy of this shape in the codebase and still not the extraction
/// trigger: ADR-038 decision 4 sets that at two policies needing the same
/// <em>non-trivial</em> rule, and this one is a <c>MAX</c>.
/// </para>
/// </remarks>
public interface ISubmissionNumberingPolicy
{
    Task<NextSequence> GetNextPublishSequenceNumberAsync(
        RegulatoryApplicationId applicationId,
        CancellationToken cancellationToken);
}
