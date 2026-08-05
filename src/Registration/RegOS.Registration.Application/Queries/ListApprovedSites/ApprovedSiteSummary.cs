namespace RegOS.Registration.Application.Queries.ListApprovedSites;

/// <summary>
/// One site, and the licences of this market that name it.
/// </summary>
/// <param name="SiteName">
/// Joined from the site, never copied onto the approval — the same rule
/// <c>ManufacturingOperation</c> follows, and the reason there is no
/// manufacturer name stored anywhere in RegOS (ADR-063 §3).
/// </param>
/// <param name="Approvals">
/// <b>Several is ordinary.</b> A market with two licences may name the same
/// plant on both, and the dates will differ — each licence added it when it
/// added it.
/// </param>
public sealed record ApprovedSiteSummary(
    Guid OrganizationSiteId,
    string SiteName,
    string SiteCountryName,
    IReadOnlyList<SiteApprovalSummary> Approvals);

/// <param name="ApprovedOn">
/// The date the site was added to this licence — routinely later than the
/// licence itself, which is why the relationship carries a date rather than
/// being a foreign key (ADR-063 §4).
/// </param>
public sealed record SiteApprovalSummary(
    Guid SiteApprovalId,
    Guid RegistrationId,
    string? RegistrationNumber,
    string RegistrationStatus,
    DateOnly ApprovedOn);
