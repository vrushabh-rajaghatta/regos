import { useQuery } from "@tanstack/react-query";

import { listInteractions } from "../api/listInteractions";

export function useInteractions(medicinalProductId: string) {
  return useQuery({
    queryKey: ["interactions", medicinalProductId],
    queryFn: () => listInteractions(medicinalProductId),
    enabled: !!medicinalProductId,
  });
}
