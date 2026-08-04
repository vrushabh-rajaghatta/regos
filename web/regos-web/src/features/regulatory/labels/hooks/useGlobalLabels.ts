import { useQuery } from "@tanstack/react-query";

import { listGlobalLabels } from "../api/listGlobalLabels";

export function useGlobalLabels(globalProductId: string) {
  return useQuery({
    queryKey: ["global-labels", globalProductId],
    queryFn: () => listGlobalLabels(globalProductId),
    enabled: !!globalProductId,
  });
}
