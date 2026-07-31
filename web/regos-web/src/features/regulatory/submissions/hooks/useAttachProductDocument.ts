import { useMutation, useQueryClient } from "@tanstack/react-query";

import { attachProductDocument } from "../api/attachProductDocument";

export interface AttachProductDocumentVariables {
  productDocumentId: string;
  /** Attach and place in one step — "put this into 3.2.S.2" is one action. */
  templateSectionId?: string;
}

export function useAttachProductDocument(submissionId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      productDocumentId,
      templateSectionId,
    }: AttachProductDocumentVariables) =>
      attachProductDocument(submissionId, productDocumentId, templateSectionId),

    onSuccess: () => {
      // The dossier list gains a row; the picker loses one.
      queryClient.invalidateQueries({
        queryKey: ["submissions", submissionId, "documents"],
      });
      queryClient.invalidateQueries({
        queryKey: ["submissions", submissionId, "attachable-documents"],
      });
      // And what the dossier contains has changed, so both read models the
      // workspace composes are now stale.
      queryClient.invalidateQueries({
        queryKey: ["submissions", submissionId, "content-plan"],
      });
      queryClient.invalidateQueries({
        queryKey: ["submissions", submissionId, "validation"],
      });
    },
  });
}
