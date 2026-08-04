import { useQuery } from "@tanstack/react-query";

import { listPacks } from "../api/listPacks";

export function usePacks(medicinalProductId: string) {
  return useQuery({
    queryKey: ["packs", medicinalProductId],
    queryFn: () => listPacks(medicinalProductId),
    enabled: !!medicinalProductId,
  });
}
