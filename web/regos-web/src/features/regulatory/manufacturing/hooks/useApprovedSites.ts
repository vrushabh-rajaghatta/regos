import { useQuery } from "@tanstack/react-query";

import { listApprovedSites } from "../api/listApprovedSites";

/**
 * **Keyed under `["manufacturing", medicinalProductId, "approved"]`** — the
 * same prefix the operations read uses, so the mutations on either side
 * refresh both by prefix. The alternative is two hooks that each have to
 * remember the other's key, and the one that forgets shows a stale divergence.
 */
export function useApprovedSites(medicinalProductId: string) {
  return useQuery({
    queryKey: ["manufacturing", medicinalProductId, "approved"],
    queryFn: () => listApprovedSites(medicinalProductId),
    enabled: !!medicinalProductId,
  });
}
