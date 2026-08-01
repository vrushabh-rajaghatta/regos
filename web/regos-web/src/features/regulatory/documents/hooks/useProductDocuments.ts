import { useQuery } from "@tanstack/react-query";

import { listProductDocuments } from "../api/listProductDocuments";

export function useProductDocuments(globalProductId: string) {
  return useQuery({
    queryKey: ["products", globalProductId, "documents"],
    queryFn: () => listProductDocuments(globalProductId),
    enabled: !!globalProductId,
  });
}
