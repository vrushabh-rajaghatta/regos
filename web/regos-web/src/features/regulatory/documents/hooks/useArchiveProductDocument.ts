import { useMutation, useQueryClient } from "@tanstack/react-query";

import { archiveProductDocument } from "../api/archiveProductDocument";

export function useArchiveProductDocument(
  globalProductId: string,
  documentId: string
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => archiveProductDocument(globalProductId, documentId),

    onSuccess: () => {
      // Refresh both the workspace detail (status badge + actions) and the
      // product's document list (status column).
      queryClient.invalidateQueries({
        queryKey: ["products", globalProductId, "documents", documentId],
      });
      queryClient.invalidateQueries({
        queryKey: ["products", globalProductId, "documents"],
      });
    },
  });
}
