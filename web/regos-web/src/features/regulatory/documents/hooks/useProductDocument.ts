import { useQuery } from "@tanstack/react-query";

import { getProductDocument } from "../api/getProductDocument";

export function useProductDocument(productId: string, documentId: string) {
  return useQuery({
    queryKey: ["products", productId, "documents", documentId],
    queryFn: () => getProductDocument(productId, documentId),
    enabled: !!productId && !!documentId,
  });
}
