namespace RegOS.Study.Application.Queries.ListStudies;

/// <param name="Kind">
/// Which aggregate the row came from — <see cref="Clinical"/> or
/// <see cref="NonClinical"/>.
/// </param>
/// <remarks>
/// <b>A string, deliberately, and not a <c>StudyKind</c> enum.</b> The two
/// studies are separate aggregates with no shared parent (ADR-056 §2), and a
/// kind type would be the discriminator that decision declined — harmless in a
/// DTO, and one refactor away from the domain. This row is assembled by a read
/// that composes across both sets (ADR-039 principle 7, the same move ADR-040 §3
/// made for the interaction timeline), so the label belongs to the read.
/// </remarks>
public sealed record StudySummary(
    Guid Id,
    string Kind,
    string SponsorStudyIdentifier,
    string Title,
    DateTime CreatedOn)
{
    public const string Clinical = "Clinical";

    public const string NonClinical = "NonClinical";
}
