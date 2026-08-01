import { useMutation, useQueryClient } from "@tanstack/react-query";

import { archiveProduct } from "../api/archiveProduct";

export function useArchiveProduct(globalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => archiveProduct(globalProductId),

    onSuccess: () => {
      // The directory hides archived products, so the list must refetch too.
      queryClient.invalidateQueries({ queryKey: ["products"] });
    },
  });
}
