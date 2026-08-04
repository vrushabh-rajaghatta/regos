import { useMutation, useQueryClient } from "@tanstack/react-query";

import { restateComponent } from "../api/restateComponent";
import type { ComponentBody } from "../types/Component";

export function useRestateComponent(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: ComponentBody & { componentId: string }) =>
      restateComponent(input.componentId, {
        componentTypeCode: input.componentTypeCode,
        name: input.name,
        description: input.description,
        quantity: input.quantity,
        unitOfPresentationCode: input.unitOfPresentationCode,
        doseFormCode: input.doseFormCode,
      }),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["components", medicinalProductId],
      });
    },
  });
}
