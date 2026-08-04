import { useMutation, useQueryClient } from "@tanstack/react-query";

import { discardLocalLabelDraft } from "../api/discardLocalLabelDraft";

export function useDiscardLocalLabelDraft() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (localLabelId: string) => discardLocalLabelDraft(localLabelId),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["local-labels"] });
    },
  });
}
