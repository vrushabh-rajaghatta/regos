import { useMutation, useQueryClient } from "@tanstack/react-query";

import { publishLocalLabelRevision } from "../api/publishLocalLabelRevision";
import type { PublishLocalLabelRevisionBody } from "../api/publishLocalLabelRevision";

interface PublishInput extends PublishLocalLabelRevisionBody {
  localLabelId: string;
  revisionId: string;
}

export function usePublishLocalLabelRevision() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: PublishInput) =>
      publishLocalLabelRevision(input.localLabelId, input.revisionId, {
        approvedOn: input.approvedOn,
        effectiveFrom: input.effectiveFrom,
      }),

    // One publish changes two rows — the new revision and the one it retired.
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["local-labels"] });
    },
  });
}
