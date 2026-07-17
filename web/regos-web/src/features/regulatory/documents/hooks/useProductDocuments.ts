import { useQuery } from "@tanstack/react-query";

import { listProductDocuments } from "../api/listProductDocuments";

export function useProductDocuments(productId: string) {
  return useQuery({
    queryKey: ["products", productId, "documents"],
    queryFn: () => listProductDocuments(productId),
    enabled: !!productId,
  });
}
