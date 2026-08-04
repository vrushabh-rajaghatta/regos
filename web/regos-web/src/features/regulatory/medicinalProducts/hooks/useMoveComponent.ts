import { useMutation, useQueryClient } from "@tanstack/react-query";

import { moveComponent } from "../api/moveComponent";

export function useMoveComponent(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: {
      componentId: string;
      newParentComponentId: string | null;
    }) => moveComponent(input.componentId, input.newParentComponentId),

    // The whole list re-reads: a move changes the depth and order of a subtree,
    // not just the row that moved.
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["components", medicinalProductId],
      });
    },
  });
}
