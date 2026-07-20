import { useMutation, useQueryClient } from "@tanstack/react-query";

import { deactivateUser } from "../api/deactivateUser";

export function useDeactivateUser(userId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => deactivateUser(userId),

    // Re-read the user so the screen reflects the source of truth.
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });
}
