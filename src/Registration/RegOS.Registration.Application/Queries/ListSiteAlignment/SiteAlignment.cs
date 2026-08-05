namespace RegOS.Registration.Application.Queries.ListSiteAlignment;

/// <summary>
/// One site, what it does for this market, and whether a licence names it.
/// </summary>
/// <remarks>
/// <b>Two facts and no verdict, and the omission is deliberate.</b> EPIC-022's
/// stability read exposes a derived <c>StabilitySupported</c> because the rule
/// behind it — <em>any overlap between two sets of conditions</em> — is
/// non-trivial and had to live in exactly one place. The rule here is
/// <c>Manufactures &amp;&amp; Approved</c>. Naming a third field for it would
/// add something to keep in sync with the two that already say it, which is the
/// call <c>ExpiryVisibility</c> made when it refused to ship an
/// <c>IsExpiringSoon</c>.
/// </remarks>
/// <param name="Operations">
/// The <b>current</b> operations this site performs for this market. Closed
/// periods are excluded: a site that stopped in 2023 is history, not a
/// divergence, and raising an advisory about it would make every transfer look
/// like a finding.
/// </param>
/// <param name="Manufactures">
/// Whether the site currently performs any operation here.
/// </param>
/// <param name="Approved">
/// Whether any of this market's licences names the site.
/// </param>
public sealed record SiteAlignment(
    Guid OrganizationSiteId,
    string SiteName,
    string SiteCountryName,
    IReadOnlyList<string> Operations,
    IReadOnlyList<SiteAlignmentApproval> Approvals,
    bool Manufactures,
    bool Approved);

public sealed record SiteAlignmentApproval(
    string? RegistrationNumber,
    DateOnly ApprovedOn);
