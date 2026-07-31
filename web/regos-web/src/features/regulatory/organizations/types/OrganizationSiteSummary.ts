import type { SiteIdentifier } from "./SiteIdentifier";

/** A site as its owning organization lists it — the company is implied. */
export interface OrganizationSiteSummary {
  siteId: string;
  name: string;
  type: string;
  countryId: string;
  countryName: string;
  city: string | null;
  status: string;
  statusDate: string;
  identifiers: SiteIdentifier[];
}
