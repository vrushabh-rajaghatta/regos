import { useMutation, useQueryClient } from "@tanstack/react-query";

import { updateProduct, type UpdateProductRequest } from "../api/updateProduct";

export function useUpdateProduct(productId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateProductRequest) =>
      updateProduct(productId, request),

    onSuccess: () => {
      // Both the detail view and the list show name and type.
      queryClient.invalidateQueries({ queryKey: ["products"] });
    },
  });
}
