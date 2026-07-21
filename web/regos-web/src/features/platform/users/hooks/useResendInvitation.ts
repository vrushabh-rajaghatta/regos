import { useMutation, useQueryClient } from "@tanstack/react-query";

import { resendInvitation } from "../api/resendInvitation";

export function useResendInvitation(userId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => resendInvitation(userId),

    // The user's status does not change, but the invitation behind it does, so
    // re-read rather than assume nothing moved.
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });
}
