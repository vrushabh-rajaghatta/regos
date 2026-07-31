import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { SiteDirectoryEntry } from "../types/SiteDirectoryEntry";

export interface SiteDirectoryFilters {
  countryId?: string;
  type?: string;
}

/**
 * Both filters are optional and neither is a default. Nothing is hidden —
 * inactive sites come back marked, because a site that closed last year is
 * still the site named on a licence granted in 2019.
 */
export async function siteDirectory(
  filters: SiteDirectoryFilters = {},
): Promise<SiteDirectoryEntry[]> {
  const query = new URLSearchParams();

  if (filters.countryId) query.set("countryId", filters.countryId);
  if (filters.type) query.set("type", filters.type);

  const suffix = query.size > 0 ? `?${query}` : "";

  const response = await apiFetch(
    buildUrl(`/api/organization-sites${suffix}`),
  );

  if (!response.ok) {
    throw new Error("Unable to load the site directory.");
  }

  return response.json();
}
