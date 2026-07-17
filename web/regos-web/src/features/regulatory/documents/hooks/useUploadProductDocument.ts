import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  uploadProductDocument,
  type UploadProductDocumentRequest,
} from "../api/uploadProductDocument";

export function useUploadProductDocument(productId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UploadProductDocumentRequest) =>
      uploadProductDocument(productId, request),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["products", productId, "documents"],
      });
    },
  });
}
