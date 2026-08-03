using RegOS.ReferenceData.Domain.SubmissionSubType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Domain.Tests.Submission;

/// <summary>
/// A classification for the tests that are not about classification.
/// </summary>
/// <remarks>
/// Eight test files construct a submission to exercise documents, placement,
/// publication, roles or format, and none of them cares which regulatory
/// activity it belongs to — but S003 made that argument required, for the same
/// reason format is required: the filer decides, and the model must not answer
/// for them.
/// <para>
/// Shared rather than repeated eight times, which is the demonstrated need
/// ADR-018 asks for. A test that is <em>about</em> the classification builds its
/// own and names the values it depends on.
/// </para>
/// </remarks>
internal static class SubmissionClassifications
{
    /// <summary>
    /// Opens an activity of some type. The ids are arbitrary on purpose — a
    /// test that asserts on them is testing the wrong thing here.
    /// </summary>
    public static SubmissionClassification Any() =>
        SubmissionClassification.Opens(
            SubmissionTypeId.New(),
            SubmissionSubTypeId.New());
}
