import { useMutation, useQueryClient } from "@tanstack/react-query";

import { registerProduct } from "../api/registerProduct";

export function useRegisterProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: registerProduct,

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["products"],
      });
    },
  });
}
