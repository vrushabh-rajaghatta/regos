import { useMutation, useQueryClient } from "@tanstack/react-query";

import { activateProductDocument } from "../api/activateProductDocument";

export function useActivateProductDocument(
  productId: string,
  documentId: string
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => activateProductDocument(productId, documentId),

    onSuccess: () => {
      // Refresh both the workspace detail (status badge + actions) and the
      // product's document list (status column).
      queryClient.invalidateQueries({
        queryKey: ["products", productId, "documents", documentId],
      });
      queryClient.invalidateQueries({
        queryKey: ["products", productId, "documents"],
      });
    },
  });
}
