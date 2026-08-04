import { useQuery } from "@tanstack/react-query";

import { listCoreVersionsForProduct } from "../api/listCoreVersionsForProduct";

export function useCoreVersionsForProduct(globalProductId: string) {
  return useQuery({
    queryKey: ["global-labels", globalProductId, "core-versions"],
    queryFn: () => listCoreVersionsForProduct(globalProductId),
    enabled: !!globalProductId,
  });
}
