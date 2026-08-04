import { useMutation, useQueryClient } from "@tanstack/react-query";

import { saveIndicationPopulation } from "../api/saveIndicationPopulation";
import type { PopulationBody } from "../types/Indication";

interface SaveInput {
  indicationId: string;
  /** Null adds; an id **amends that qualifier in place** (EPIC-018 D2). */
  populationId: string | null;
  body: PopulationBody;
}

export function useSaveIndicationPopulation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: SaveInput) =>
      saveIndicationPopulation(
        input.indicationId,
        input.populationId,
        input.body,
      ),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["indications"] });
    },
  });
}
