import { useMutation, useQueryClient } from "@tanstack/react-query";

import { saveStatementPopulation } from "../api/saveStatementPopulation";
import type { PopulationBody } from "../types/Indication";
import type { StatementKind } from "../types/StatementKind";

interface SaveInput {
  kind: StatementKind;
  statementId: string;
  /** Null adds; an id **amends that qualifier in place** (EPIC-018 D2). */
  populationId: string | null;
  body: PopulationBody;
}

export function useSaveStatementPopulation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: SaveInput) =>
      saveStatementPopulation(
        input.kind,
        input.statementId,
        input.populationId,
        input.body,
      ),

    onSuccess: (_result, input) => {
      queryClient.invalidateQueries({ queryKey: [input.kind] });
    },
  });
}
