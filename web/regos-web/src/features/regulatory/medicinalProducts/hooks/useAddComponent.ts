import { useMutation, useQueryClient } from "@tanstack/react-query";

import { addComponent } from "../api/addComponent";
import type { ComponentBody } from "../types/Component";

export function useAddComponent(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: ComponentBody & { parentComponentId: string | null }) =>
      addComponent(medicinalProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["components", medicinalProductId],
      });
    },
  });
}
