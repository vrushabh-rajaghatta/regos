import { useQuery } from "@tanstack/react-query";

import { getProductDocumentUsage } from "../api/getProductDocumentUsage";

export function useProductDocumentUsage(
  productId: string,
  documentId: string
) {
  return useQuery({
    queryKey: ["products", productId, "documents", documentId, "usage"],
    queryFn: () => getProductDocumentUsage(productId, documentId),
    enabled: !!productId && !!documentId,
  });
}
