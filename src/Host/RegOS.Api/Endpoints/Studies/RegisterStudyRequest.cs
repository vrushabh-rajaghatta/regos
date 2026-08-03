namespace RegOS.Api.Endpoints.Studies;

/// <param name="SponsorStudyIdentifier">
/// The sponsor's own code for the study. Called <b>Study ID</b> on screen and in
/// the eCTD, and deliberately not called that here — it is not this record's
/// identity (ADR-056).
/// </param>
/// <remarks>
/// One request record for both kinds, because the two routes take the same two
/// facts. It is a wire shape rather than a domain type, so sharing it does not
/// give the aggregates a common parent — and if the kinds' fields diverge, this
/// splits with them.
/// </remarks>
public sealed record RegisterStudyRequest(
    string SponsorStudyIdentifier,
    string Title);
