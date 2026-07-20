import { useMutation, useQueryClient } from "@tanstack/react-query";

import { activateUser } from "../api/activateUser";

export function useActivateUser(userId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => activateUser(userId),

    // Re-read the user after success so the screen reflects the source of
    // truth rather than an assumed status.
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });
}
