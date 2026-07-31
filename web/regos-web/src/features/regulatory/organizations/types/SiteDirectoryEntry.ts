import type { SiteIdentifier } from "./SiteIdentifier";

/**
 * A row in the tenant-wide site directory. Carries the organization, because
 * the directory spans the whole registry and the owning company is not implied
 * by where you are standing — that is the question which made OrganizationSite
 * an aggregate root rather than a child of Organization.
 */
export interface SiteDirectoryEntry {
  siteId: string;
  name: string;
  type: string;
  organizationId: string;
  organizationName: string;
  countryId: string;
  countryName: string;
  city: string | null;
  status: string;
  statusDate: string;
  identifiers: SiteIdentifier[];
}
