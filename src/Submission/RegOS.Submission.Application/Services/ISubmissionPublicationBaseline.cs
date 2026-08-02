using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Submission.Application.Services;

/// <summary>
/// What the next filing in an application follows: the number it takes, and the
/// dossier it is measured against.
/// </summary>
/// <remarks>
/// One question, deliberately. S001 asked only for the number, and S002 needs
/// the previous sequence's placements to derive the operation — the same read,
/// one join deeper. Two services both asking <em>what came before?</em> would be
/// two chances to disagree about the answer.
/// <para>
/// A <c>Submission</c> is a root, so the sequence before it is outside its
/// consistency boundary and this necessarily lives in the application layer
/// (ADR-044 decision 6). <b>Nothing is reserved</b> — the answer is true when
/// read and a concurrent publish may take the number before the caller uses it.
/// The unique index arbitrates that; the caller is told to try again.
/// </para>
/// </remarks>
public interface ISubmissionPublicationBaseline
{
    Task<PublicationBaseline> GetAsync(
        RegulatoryApplicationId applicationId,
        CancellationToken cancellationToken);
}
