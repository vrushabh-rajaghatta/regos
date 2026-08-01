import { useMutation, useQueryClient } from "@tanstack/react-query";

import { activateMedicinalProduct } from "../api/activateMedicinalProduct";
import { deactivateMedicinalProduct } from "../api/deactivateMedicinalProduct";

/**
 * One hook for both directions. The two calls carry the same single fact and
 * differ only in which way the flag moves, so two identical hooks named
 * differently would be symmetry for its own sake.
 */
export function useSetMarketActivation(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ active, on }: { active: boolean; on: string }) =>
      active
        ? activateMedicinalProduct(medicinalProductId, on)
        : deactivateMedicinalProduct(medicinalProductId, on),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["medicinal-products"] });
    },
  });
}
