import { useMutation, useQueryClient } from "@tanstack/react-query";

import { restateIngredient } from "../api/restateIngredient";
import type { IngredientBody } from "../types/Presentation";

export function useRestateIngredient(
  medicinalProductId: string,
  presentationId: string,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: IngredientBody & { ingredientId: string }) =>
      restateIngredient(presentationId, input.ingredientId, {
        role: input.role,
        numeratorValue: input.numeratorValue,
        numeratorUnitCode: input.numeratorUnitCode,
        denominatorValue: input.denominatorValue,
        denominatorUnitCode: input.denominatorUnitCode,
      }),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["presentations", medicinalProductId],
      });
    },
  });
}
