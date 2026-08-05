/**
 * One site, what it does for this market, and whether a licence names it.
 *
 * **Two facts and no verdict**, deliberately. The stability read carries a
 * derived `stabilitySupported` because the rule behind it — any overlap between
 * two sets of conditions — is non-trivial and lives in one place. The rule here
 * is `manufactures && approved`, and a third field for it would be a third
 * thing to keep in sync with the two that already say it.
 */
export interface SiteAlignment {
  organizationSiteId: string;
  siteName: string;
  siteCountryName: string;
  /** **Current** operations only — a closed period is history, not a finding. */
  operations: string[];
  approvals: { registrationNumber: string | null; approvedOn: string }[];
  manufactures: boolean;
  approved: boolean;
}
