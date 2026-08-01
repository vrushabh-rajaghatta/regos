import { useQuery } from "@tanstack/react-query";

import { getProductDocumentUsage } from "../api/getProductDocumentUsage";

export function useProductDocumentUsage(
  globalProductId: string,
  documentId: string
) {
  return useQuery({
    queryKey: ["products", globalProductId, "documents", documentId, "usage"],
    queryFn: () => getProductDocumentUsage(globalProductId, documentId),
    enabled: !!globalProductId && !!documentId,
  });
}
