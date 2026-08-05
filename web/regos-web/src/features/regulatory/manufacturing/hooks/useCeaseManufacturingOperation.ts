import { useMutation, useQueryClient } from "@tanstack/react-query";

import { ceaseManufacturingOperation } from "../api/ceaseManufacturingOperation";

export function useCeaseManufacturingOperation(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: { operationId: string; ceasedOn: string }) =>
      ceaseManufacturingOperation(input.operationId, input.ceasedOn),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["manufacturing", medicinalProductId],
      });
    },
  });
}
