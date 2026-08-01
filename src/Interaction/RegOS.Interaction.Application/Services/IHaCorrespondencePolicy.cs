using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.Regulatory.Correspondence;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Interaction.Application.Services;

/// <summary>
/// Cross-aggregate checks the aggregate cannot make for itself: that the
/// authority and type are real, that any anchor given is one this tenant can
/// see, and that <b>every referenced child reference-data object belongs to its
/// selected parent</b>.
/// </summary>
/// <remarks>
/// The fifth policy of this shape in the codebase, and still not the extraction
/// trigger — ADR-038 decision 4 sets that at <em>two of them needing the same
/// non-trivial rule</em>, not at another one appearing. What these share is one
/// line of "does this row exist".
/// <para>
/// It exists so a bad id is a 404 rather than a foreign-key 500, and so an
/// anchor belonging to another tenant is indistinguishable from one that does
/// not exist (ADR-031's fail-closed filters do the work; this turns the empty
/// result into a semantic exception).
/// </para>
/// <para>
/// <b>The child-belongs-to-parent rule is the first genuinely semantic thing a
/// creation policy does here.</b> The other five check only that a row exists;
/// this one validates a <em>relationship</em> — a letter from the FDA cannot
/// name a Health Canada bureau. Stated generally rather than as
/// <em>"the division belongs to the authority"</em>, because the invariant is
/// the shape and not the pair: if a committee or an office arrives later, the
/// same rule covers it. Per ADR-038 decision 4 this is the <b>first</b>
/// non-trivial rule of its kind, not the second, so it is not yet the
/// extraction trigger.
/// </para>
/// </remarks>
public interface IHaCorrespondencePolicy
{
    Task EnsureCanRecordAsync(
        AuthorityId authorityId,
        CorrespondenceTypeId correspondenceTypeId,
        AuthorityDivisionId? authorityDivisionId,
        RegulatoryApplicationId? regulatoryApplicationId,
        SubmissionId? submissionId,
        RegistrationId? registrationId,
        CancellationToken cancellationToken);
}
