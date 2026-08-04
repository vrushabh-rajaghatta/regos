import { useMutation, useQueryClient } from "@tanstack/react-query";

import { removeStatementPopulation } from "../api/removeStatementPopulation";
import type { StatementKind } from "../types/StatementKind";

export function useRemoveStatementPopulation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: {
      kind: StatementKind;
      statementId: string;
      populationId: string;
    }) =>
      removeStatementPopulation(
        input.kind,
        input.statementId,
        input.populationId,
      ),

    onSuccess: (_result, input) => {
      queryClient.invalidateQueries({ queryKey: [input.kind] });
    },
  });
}
