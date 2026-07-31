import { useQuery } from "@tanstack/react-query";

import {
  siteDirectory,
  type SiteDirectoryFilters,
} from "../api/siteDirectory";

export function useSiteDirectory(filters: SiteDirectoryFilters = {}) {
  return useQuery({
    queryKey: ["site-directory", filters.countryId ?? null, filters.type ?? null],
    queryFn: () => siteDirectory(filters),
  });
}
