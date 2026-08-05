/**
 * One site, and the licences of this market that name it.
 *
 * **The site's name is joined, never copied** — the same rule
 * `ManufacturingOperation` follows, and the reason there is no manufacturer
 * name stored anywhere in RegOS (ADR-063 §3).
 */
export interface ApprovedSite {
  organizationSiteId: string;
  siteName: string;
  siteCountryName: string;
  /**
   * **Several is ordinary.** A market with two licences may name the same plant
   * on both, and the dates will differ — each licence added it when it did.
   */
  approvals: SiteApproval[];
}

export interface SiteApproval {
  siteApprovalId: string;
  registrationId: string;
  registrationNumber: string | null;
  registrationStatus: string;
  /**
   * Routinely later than the licence itself — a site joins by variation, years
   * after the authorisation was granted.
   */
  approvedOn: string;
}
