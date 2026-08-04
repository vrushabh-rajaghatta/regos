import { useMutation, useQueryClient } from "@tanstack/react-query";

import { removeIngredient } from "../api/removeIngredient";

export function useRemoveIngredient(
  medicinalProductId: string,
  presentationId: string,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (ingredientId: string) =>
      removeIngredient(presentationId, ingredientId),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["presentations", medicinalProductId],
      });
    },
  });
}
