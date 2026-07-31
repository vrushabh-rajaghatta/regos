import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  createRegulatoryApplication,
  type CreateRegulatoryApplicationRequest,
} from "../api/createRegulatoryApplication";

export function useCreateRegulatoryApplication(globalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateRegulatoryApplicationRequest) =>
      createRegulatoryApplication(globalProductId, request),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["products", globalProductId, "applications"],
      });
    },
  });
}
