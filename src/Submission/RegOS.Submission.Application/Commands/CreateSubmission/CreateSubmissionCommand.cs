using RegOS.ReferenceData.Domain.SubmissionSubType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.CreateSubmission;

/// <param name="SubmissionSubTypeId">
/// What this sequence does to its regulatory activity. Required, and supplied
/// rather than inferred — see <c>SubmissionSubType</c> for why position cannot
/// give it (evidence E13).
/// </param>
/// <param name="SubmissionTypeId">
/// What activity this sequence <em>starts</em>. Set this or
/// <paramref name="OriginatingSubmissionId"/>, never both.
/// </param>
/// <param name="OriginatingSubmissionId">
/// The published sequence that opened the activity this one <em>continues</em>.
/// </param>
/// <remarks>
/// <b>The command can express a contradiction and the domain cannot</b> — both
/// set, or neither. That asymmetry is deliberate: a command is shaped by what
/// arrives over the wire, and refusing nonsense at the boundary is what lets
/// <c>SubmissionClassification</c> be a type with only two ways to build it.
/// </remarks>
public sealed record CreateSubmissionCommand(
    RegulatoryApplicationId ApplicationId,
    string Title,
    SubmissionFormat Format,
    SubmissionSubTypeId SubmissionSubTypeId,
    SubmissionTypeId? SubmissionTypeId = null,
    SubmissionId? OriginatingSubmissionId = null);
