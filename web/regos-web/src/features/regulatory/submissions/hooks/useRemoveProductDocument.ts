import { useMutation, useQueryClient } from "@tanstack/react-query";

import { removeProductDocument } from "../api/removeProductDocument";

export function useRemoveProductDocument(submissionId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (submissionDocumentId: string) =>
      removeProductDocument(submissionId, submissionDocumentId),

    onSuccess: () => {
      // The dossier list loses a row; the picker may gain one back.
      queryClient.invalidateQueries({
        queryKey: ["submissions", submissionId, "documents"],
      });
      queryClient.invalidateQueries({
        queryKey: ["submissions", submissionId, "attachable-documents"],
      });
    },
  });
}
