import { useQuery } from "@tanstack/react-query";

import { getProductDocument } from "../api/getProductDocument";

export function useProductDocument(globalProductId: string, documentId: string) {
  return useQuery({
    queryKey: ["products", globalProductId, "documents", documentId],
    queryFn: () => getProductDocument(globalProductId, documentId),
    enabled: !!globalProductId && !!documentId,
  });
}
