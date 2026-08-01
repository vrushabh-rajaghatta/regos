import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  uploadProductDocument,
  type UploadProductDocumentRequest,
} from "../api/uploadProductDocument";

export function useUploadProductDocument(globalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UploadProductDocumentRequest) =>
      uploadProductDocument(globalProductId, request),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["products", globalProductId, "documents"],
      });
    },
  });
}
