using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.Regulatory.Correspondence;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Interaction.Application.Services;

/// <summary>
/// Cross-aggregate existence checks the aggregate cannot make for itself: that
/// the authority and type are real, and that any anchor given is one this
/// tenant can see.
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
/// </remarks>
public interface IHaCorrespondencePolicy
{
    Task EnsureCanRecordAsync(
        AuthorityId authorityId,
        CorrespondenceTypeId correspondenceTypeId,
        RegulatoryApplicationId? regulatoryApplicationId,
        SubmissionId? submissionId,
        RegistrationId? registrationId,
        CancellationToken cancellationToken);
}
