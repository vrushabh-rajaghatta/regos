import { useMutation, useQueryClient } from "@tanstack/react-query";

import { startLocalLabelRevision } from "../api/startLocalLabelRevision";

export function useStartLocalLabelRevision() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (localLabelId: string) => startLocalLabelRevision(localLabelId),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["local-labels"] });
    },
  });
}
