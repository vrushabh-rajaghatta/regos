import { useMutation, useQueryClient } from "@tanstack/react-query";

import { attachProductDocument } from "../api/attachProductDocument";

export function useAttachProductDocument(submissionId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (productDocumentId: string) =>
      attachProductDocument(submissionId, productDocumentId),

    onSuccess: () => {
      // The dossier list gains a row; the picker loses one.
      queryClient.invalidateQueries({
        queryKey: ["submissions", submissionId, "documents"],
      });
      queryClient.invalidateQueries({
        queryKey: ["submissions", submissionId, "attachable-documents"],
      });
    },
  });
}
