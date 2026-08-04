import { useMutation, useQueryClient } from "@tanstack/react-query";

import { publishGlobalLabelVersion } from "../api/publishGlobalLabelVersion";
import type { PublishGlobalLabelVersionBody } from "../api/publishGlobalLabelVersion";

interface PublishInput extends PublishGlobalLabelVersionBody {
  globalLabelId: string;
  versionId: string;
}

export function usePublishGlobalLabelVersion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: PublishInput) =>
      publishGlobalLabelVersion(input.globalLabelId, input.versionId, {
        effectiveFrom: input.effectiveFrom,
        changeSummary: input.changeSummary,
      }),

    // One publish changes two rows in the history — the new version and the one
    // it retired — so the whole prefix goes.
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["global-labels"] });
    },
  });
}
