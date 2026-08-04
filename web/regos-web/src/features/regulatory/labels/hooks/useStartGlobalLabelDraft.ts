import { useMutation, useQueryClient } from "@tanstack/react-query";

import { startGlobalLabelDraft } from "../api/startGlobalLabelDraft";

export function useStartGlobalLabelDraft() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (globalLabelId: string) => startGlobalLabelDraft(globalLabelId),

    // Both the row, which now shows a draft, and the history behind it.
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["global-labels"] });
    },
  });
}
