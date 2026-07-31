import { useQuery } from "@tanstack/react-query";

import { getProduct, ProductNotFoundError } from "../api/getProduct";

export function useProduct(globalProductId: string) {
  return useQuery({
    queryKey: ["products", globalProductId],
    queryFn: () => getProduct(globalProductId),
    enabled: !!globalProductId,
    // A missing product will still be missing on the third attempt; only
    // transport failures are worth retrying.
    retry: (failureCount, error) =>
      !(error instanceof ProductNotFoundError) && failureCount < 2,
  });
}
