import { useQuery } from "@tanstack/react-query";

import { getProduct, ProductNotFoundError } from "../api/getProduct";

export function useProduct(productId: string) {
  return useQuery({
    queryKey: ["products", productId],
    queryFn: () => getProduct(productId),
    enabled: !!productId,
    // A missing product will still be missing on the third attempt; only
    // transport failures are worth retrying.
    retry: (failureCount, error) =>
      !(error instanceof ProductNotFoundError) && failureCount < 2,
  });
}
