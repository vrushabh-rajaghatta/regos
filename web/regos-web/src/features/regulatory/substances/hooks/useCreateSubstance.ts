import { useMutation, useQueryClient } from "@tanstack/react-query";

import { createSubstance } from "../api/createSubstance";
import type { CreateSubstanceBody } from "../api/createSubstance";

export function useCreateSubstance() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CreateSubstanceBody) => createSubstance(input),

    // Every search and filter combination shows the new row, so the whole key
    // prefix is invalidated rather than the one list the user happens to be
    // looking at.
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["substances"] });
    },
  });
}
