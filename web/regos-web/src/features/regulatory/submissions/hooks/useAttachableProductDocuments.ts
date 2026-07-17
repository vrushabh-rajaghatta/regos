import { useQuery } from "@tanstack/react-query";

import { listAttachableProductDocuments } from "../api/listAttachableProductDocuments";

export function useAttachableProductDocuments(
  submissionId: string,
  enabled: boolean
) {
  return useQuery({
    queryKey: ["submissions", submissionId, "attachable-documents"],
    queryFn: () => listAttachableProductDocuments(submissionId),
    // Only fetched when the picker is open.
    enabled: !!submissionId && enabled,
  });
}
