import { useMutation, useQueryClient } from "@tanstack/react-query";

import { restatePresentation } from "../api/restatePresentation";
import type { PresentationBody } from "../types/Presentation";

export function useRestatePresentation(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: PresentationBody & { presentationId: string }) =>
      restatePresentation(input.presentationId, {
        name: input.name,
        description: input.description,
        doseFormCode: input.doseFormCode,
        unitOfPresentationCode: input.unitOfPresentationCode,
        routeCodes: input.routeCodes,
      }),

    // Keyed by the market, not the presentation: the panel that shows the
    // corrected row is the market's list.
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["presentations", medicinalProductId],
      });
    },
  });
}
