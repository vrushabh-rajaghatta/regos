import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  recordManufacturingOperation,
  type ManufacturingOperationBody,
} from "../api/recordManufacturingOperation";

export function useRecordManufacturingOperation(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: ManufacturingOperationBody) =>
      recordManufacturingOperation(medicinalProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["manufacturing", medicinalProductId],
      });
    },
  });
}
