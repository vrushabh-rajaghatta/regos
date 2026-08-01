import { useMutation, useQueryClient } from "@tanstack/react-query";

import { createMedicinalProduct } from "../api/createMedicinalProduct";
import type { CreateMedicinalProductBody } from "../api/createMedicinalProduct";

export function useCreateMedicinalProduct(globalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: CreateMedicinalProductBody) =>
      createMedicinalProduct(globalProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["medicinal-products"] });
    },
  });
}
