import { useQuery } from "@tanstack/react-query";

import { listGlobalLabelVersions } from "../api/listGlobalLabelVersions";

/**
 * Asked for rather than always fetched — the history is opened per label, and
 * a product with four labels should not load four version lists to show a list.
 */
export function useGlobalLabelVersions(
  globalLabelId: string,
  enabled: boolean,
) {
  return useQuery({
    queryKey: ["global-labels", globalLabelId, "versions"],
    queryFn: () => listGlobalLabelVersions(globalLabelId),
    enabled: enabled && !!globalLabelId,
  });
}
