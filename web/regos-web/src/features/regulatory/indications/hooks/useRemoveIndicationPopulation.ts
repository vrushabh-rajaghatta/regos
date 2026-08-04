import { useMutation, useQueryClient } from "@tanstack/react-query";

import { removeIndicationPopulation } from "../api/removeIndicationPopulation";

export function useRemoveIndicationPopulation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: { indicationId: string; populationId: string }) =>
      removeIndicationPopulation(input.indicationId, input.populationId),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["indications"] });
    },
  });
}
