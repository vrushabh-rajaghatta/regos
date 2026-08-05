import { useQuery } from "@tanstack/react-query";

import { listAuthorisedPacks } from "../api/listAuthorisedPacks";

/**
 * **Keyed under `["packs", medicinalProductId]` on purpose.**
 *
 * This is a second view over the same packs, not a separate resource, and
 * TanStack invalidates by key prefix — so every existing pack mutation (add,
 * restate, marketing status, supply, layers) refreshes it without having been
 * told this panel exists. The alternative is five hooks that each have to
 * remember a sixth key, and the one that forgets shows a stale screen.
 */
export function useAuthorisedPacks(medicinalProductId: string) {
  return useQuery({
    queryKey: ["packs", medicinalProductId, "authorised"],
    queryFn: () => listAuthorisedPacks(medicinalProductId),
    enabled: !!medicinalProductId,
  });
}
