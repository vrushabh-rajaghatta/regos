import { useQuery } from "@tanstack/react-query";

import { listProductRegistrations } from "../api/listProductRegistrations";

export function useProductRegistrations(globalProductId: string) {
  return useQuery({
    queryKey: ["registrations", "product", globalProductId],
    queryFn: () => listProductRegistrations(globalProductId),
    enabled: !!globalProductId,
  });
}
