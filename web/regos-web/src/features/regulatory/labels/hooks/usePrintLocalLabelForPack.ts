import { useMutation, useQueryClient } from "@tanstack/react-query";

import { printLocalLabelForPack } from "../api/printLocalLabelForPack";

export function usePrintLocalLabelForPack() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      localLabelId,
      packagedProductId,
    }: {
      localLabelId: string;
      packagedProductId: string | null;
    }) => printLocalLabelForPack(localLabelId, packagedProductId),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["local-labels"] });
    },
  });
}
