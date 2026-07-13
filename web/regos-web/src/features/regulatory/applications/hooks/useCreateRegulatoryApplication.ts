import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  createRegulatoryApplication,
  type CreateRegulatoryApplicationRequest,
} from "../api/createRegulatoryApplication";

export function useCreateRegulatoryApplication(productId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateRegulatoryApplicationRequest) =>
      createRegulatoryApplication(productId, request),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["products", productId, "applications"],
      });
    },
  });
}
