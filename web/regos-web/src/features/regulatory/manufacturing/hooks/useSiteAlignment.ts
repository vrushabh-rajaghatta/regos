import { useQuery } from "@tanstack/react-query";

import { listSiteAlignment } from "../api/listSiteAlignment";

/**
 * **Keyed under the `["manufacturing", medicinalProductId]` prefix**, so every
 * mutation on either side of the comparison — recording an operation, closing
 * one, approving a site, removing an approval — refreshes it without having
 * been told this panel exists.
 */
export function useSiteAlignment(medicinalProductId: string) {
  return useQuery({
    queryKey: ["manufacturing", medicinalProductId, "alignment"],
    queryFn: () => listSiteAlignment(medicinalProductId),
    enabled: !!medicinalProductId,
  });
}
