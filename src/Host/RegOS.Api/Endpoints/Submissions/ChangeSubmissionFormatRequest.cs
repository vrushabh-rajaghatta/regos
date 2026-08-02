namespace RegOS.Api.Endpoints.Submissions;

/// <param name="Format">
/// <c>Ectd</c>, <c>Nees</c> or <c>Paper</c> — the domain's words. The screen
/// says "eCTD", "NeeS" and "Paper", and the mapping between the two lives in
/// the client.
/// </param>
public sealed record ChangeSubmissionFormatRequest(
    string Format);
