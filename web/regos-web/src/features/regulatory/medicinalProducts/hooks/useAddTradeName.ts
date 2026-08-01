import { useMutation, useQueryClient } from "@tanstack/react-query";

import { addTradeName } from "../api/addTradeName";
import type { AddTradeNameBody } from "../api/addTradeName";

export function useAddTradeName(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: AddTradeNameBody) =>
      addTradeName(medicinalProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["medicinal-products"] });
    },
  });
}
