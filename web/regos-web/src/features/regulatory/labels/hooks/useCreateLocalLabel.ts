import { useMutation, useQueryClient } from "@tanstack/react-query";

import { createLocalLabel } from "../api/createLocalLabel";
import type { CreateLocalLabelBody } from "../api/createLocalLabel";

export function useCreateLocalLabel(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: CreateLocalLabelBody) =>
      createLocalLabel(medicinalProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["local-labels"] });
    },
  });
}
