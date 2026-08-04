import { useQuery } from "@tanstack/react-query";

import { listLocalLabels } from "../api/listLocalLabels";

export function useLocalLabels(medicinalProductId: string) {
  return useQuery({
    queryKey: ["local-labels", medicinalProductId],
    queryFn: () => listLocalLabels(medicinalProductId),
    enabled: !!medicinalProductId,
  });
}
