import { useQuery } from "@tanstack/react-query";

import { listLocalLabelRevisions } from "../api/listLocalLabelRevisions";

/** Asked for rather than always fetched — one history per label, on request. */
export function useLocalLabelRevisions(localLabelId: string, enabled: boolean) {
  return useQuery({
    queryKey: ["local-labels", localLabelId, "revisions"],
    queryFn: () => listLocalLabelRevisions(localLabelId),
    enabled: enabled && !!localLabelId,
  });
}
