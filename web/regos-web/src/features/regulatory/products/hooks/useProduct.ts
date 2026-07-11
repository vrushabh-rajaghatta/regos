import { useQuery } from "@tanstack/react-query";

import { getProduct } from "../api/getProduct";

export function useProduct(productId: string) {
  return useQuery({
    queryKey: ["products", productId],
    queryFn: () => getProduct(productId),
    enabled: !!productId,
  });
}
