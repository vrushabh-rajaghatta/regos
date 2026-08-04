import { useMutation, useQueryClient } from "@tanstack/react-query";

import { addPresentation } from "../api/addPresentation";
import type { PresentationBody } from "../types/Presentation";

export function useAddPresentation(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: PresentationBody) =>
      addPresentation(medicinalProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["presentations", medicinalProductId],
      });
    },
  });
}
