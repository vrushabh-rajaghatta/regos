import { useQuery } from "@tanstack/react-query";

import { listManufacturingOperations } from "../api/listManufacturingOperations";

/**
 * **Keyed under `["manufacturing", medicinalProductId]`**, so both mutations
 * below invalidate it by prefix and a future divergence read can join the same
 * prefix rather than being a sixth key somebody has to remember.
 */
export function useManufacturingOperations(medicinalProductId: string) {
  return useQuery({
    queryKey: ["manufacturing", medicinalProductId],
    queryFn: () => listManufacturingOperations(medicinalProductId),
    enabled: !!medicinalProductId,
  });
}
