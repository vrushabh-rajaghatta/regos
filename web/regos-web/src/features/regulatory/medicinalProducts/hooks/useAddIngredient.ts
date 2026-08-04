import { useMutation, useQueryClient } from "@tanstack/react-query";

import { addIngredient } from "../api/addIngredient";
import type { IngredientBody } from "../types/Presentation";

export function useAddIngredient(
  medicinalProductId: string,
  presentationId: string,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: IngredientBody & { substanceId: string }) =>
      addIngredient(presentationId, body),

    // Keyed by the market: ingredients arrive inside the presentation list, so
    // that is the query that goes stale.
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["presentations", medicinalProductId],
      });
    },
  });
}
