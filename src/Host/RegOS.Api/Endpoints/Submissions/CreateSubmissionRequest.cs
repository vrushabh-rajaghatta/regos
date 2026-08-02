namespace RegOS.Api.Endpoints.Submissions;

/// <param name="Format">
/// <c>Ectd</c>, <c>Nees</c> or <c>Paper</c>. Optional on the wire and defaulted
/// to eCTD — the only format an FDA IND accepts today, so an omitted value has
/// exactly one honest reading. The domain takes no default; this states one.
/// </param>
public sealed record CreateSubmissionRequest(
    Guid SubmissionTypeId,
    string Title,
    string? Format = null);
