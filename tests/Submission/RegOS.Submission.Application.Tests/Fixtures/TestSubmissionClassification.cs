using RegOS.ReferenceData.Domain.SubmissionSubType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Tests.Fixtures;

/// <summary>
/// The seeded FDA classification, for tests that must create a submission but
/// are not about which regulatory activity it belongs to.
/// </summary>
/// <remarks>
/// <b>Real seeded ids, not fresh guids.</b> These tests run against a real
/// database, and the two columns carry foreign keys — an arbitrary id would fail
/// on insert rather than on the rule under test, which is the least useful place
/// for a test to fail.
/// <para>
/// The ids are written out here for the same reason
/// <see cref="TestFdaApplication"/> writes out its own: a fixture that looked
/// them up by code would pass while the seed was wrong.
/// </para>
/// </remarks>
internal static class TestSubmissionClassification
{
    /// <summary>`fdast1` — the activity an original application opens.</summary>
    public static readonly SubmissionTypeId FdaOriginalApplication =
        new(Guid.Parse("70000000-0000-0000-0000-000000000001"));

    /// <summary>`fdasst3` — what an opening sequence usually does.</summary>
    public static readonly SubmissionSubTypeId FdaApplication =
        new(Guid.Parse("71000000-0000-0000-0000-000000000001"));

    /// <summary>`fdasst4` — what a continuing sequence usually does.</summary>
    public static readonly SubmissionSubTypeId FdaAmendment =
        new(Guid.Parse("71000000-0000-0000-0000-000000000002"));

    public static SubmissionClassification Opens() =>
        SubmissionClassification.Opens(FdaOriginalApplication, FdaApplication);
}
