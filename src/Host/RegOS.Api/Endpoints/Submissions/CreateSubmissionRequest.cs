namespace RegOS.Api.Endpoints.Submissions;

/// <param name="Format">
/// <c>Ectd</c>, <c>Nees</c> or <c>Paper</c>. Optional on the wire and defaulted
/// to eCTD — the only format an FDA IND accepts today, so an omitted value has
/// exactly one honest reading. The domain takes no default; this states one.
/// </param>
/// <param name="SubmissionSubTypeId">
/// What this sequence does to its regulatory activity. <b>Required, and
/// deliberately not defaulted</b> — unlike <paramref name="Format"/>, an omitted
/// value here has no honest reading at all: an opening sequence can perfectly
/// well be a report rather than an application (evidence E13), so there is no
/// value that "obviously" belongs.
/// </param>
/// <param name="SubmissionTypeId">
/// What regulatory activity this sequence starts. Send this or
/// <paramref name="OriginatingSubmissionId"/> — exactly one.
/// </param>
/// <param name="OriginatingSubmissionId">
/// The published sequence that opened the activity this one continues.
/// </param>
public sealed record CreateSubmissionRequest(
    string Title,
    Guid? SubmissionSubTypeId = null,
    Guid? SubmissionTypeId = null,
    Guid? OriginatingSubmissionId = null,
    string? Format = null);
