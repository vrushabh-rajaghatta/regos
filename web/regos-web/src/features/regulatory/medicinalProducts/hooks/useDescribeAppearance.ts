import { useMutation, useQueryClient } from "@tanstack/react-query";

import { describeAppearance } from "../api/describeAppearance";
import type { AppearanceBody } from "../types/Presentation";

export function useDescribeAppearance(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: AppearanceBody & { presentationId: string }) =>
      describeAppearance(input.presentationId, {
        colourCodes: input.colourCodes,
        shapeCode: input.shapeCode,
        imprint: input.imprint,
        description: input.description,
      }),

    // Keyed by the market, for the reason restate is: the panel that shows the
    // described row is the market's list.
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["presentations", medicinalProductId],
      });
    },
  });
}
