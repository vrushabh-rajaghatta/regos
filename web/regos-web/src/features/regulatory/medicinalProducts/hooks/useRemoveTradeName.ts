import { useMutation, useQueryClient } from "@tanstack/react-query";

import { removeTradeName } from "../api/removeTradeName";

export function useRemoveTradeName(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (tradeNameId: string) =>
      removeTradeName(medicinalProductId, tradeNameId),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["medicinal-products"] });
    },
  });
}
