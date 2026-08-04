import { useMutation, useQueryClient } from "@tanstack/react-query";

import { removeComponent } from "../api/removeComponent";

export function useRemoveComponent(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (componentId: string) => removeComponent(componentId),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["components", medicinalProductId],
      });
    },
  });
}
