import { useQuery } from "@tanstack/react-query";

import { listProductRegistrations } from "../api/listProductRegistrations";

export function useProductRegistrations(productId: string) {
  return useQuery({
    queryKey: ["registrations", "product", productId],
    queryFn: () => listProductRegistrations(productId),
    enabled: !!productId,
  });
}
