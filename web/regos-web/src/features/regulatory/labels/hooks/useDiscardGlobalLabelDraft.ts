import { useMutation, useQueryClient } from "@tanstack/react-query";

import { discardGlobalLabelDraft } from "../api/discardGlobalLabelDraft";

export function useDiscardGlobalLabelDraft() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (globalLabelId: string) =>
      discardGlobalLabelDraft(globalLabelId),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["global-labels"] });
    },
  });
}
