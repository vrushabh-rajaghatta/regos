import { useMutation, useQueryClient } from "@tanstack/react-query";

import { prepareLocalLabelRevision } from "../api/prepareLocalLabelRevision";
import type { PrepareLocalLabelRevisionBody } from "../api/prepareLocalLabelRevision";

interface PrepareInput extends PrepareLocalLabelRevisionBody {
  localLabelId: string;
  revisionId: string;
}

export function usePrepareLocalLabelRevision() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: PrepareInput) =>
      prepareLocalLabelRevision(input.localLabelId, input.revisionId, {
        contentId: input.contentId,
        derivedFromGlobalLabelVersionId: input.derivedFromGlobalLabelVersionId,
        dataCarrierCode: input.dataCarrierCode,
        changeSummary: input.changeSummary,
      }),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["local-labels"] });
    },
  });
}
