import { useMutation, useQueryClient } from "@tanstack/react-query";

import { createGlobalLabel } from "../api/createGlobalLabel";
import type { CreateGlobalLabelBody } from "../api/createGlobalLabel";

export function useCreateGlobalLabel(globalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: CreateGlobalLabelBody) =>
      createGlobalLabel(globalProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["global-labels", globalProductId],
      });
    },
  });
}
