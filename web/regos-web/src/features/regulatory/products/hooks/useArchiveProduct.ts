import { useMutation, useQueryClient } from "@tanstack/react-query";

import { archiveProduct } from "../api/archiveProduct";

export function useArchiveProduct(productId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => archiveProduct(productId),

    onSuccess: () => {
      // The directory hides archived products, so the list must refetch too.
      queryClient.invalidateQueries({ queryKey: ["products"] });
    },
  });
}
