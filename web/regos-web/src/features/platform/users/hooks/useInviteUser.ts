import { useMutation, useQueryClient } from "@tanstack/react-query";

import { inviteUser } from "../api/inviteUser";

export function useInviteUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: inviteUser,

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["users"],
      });
    },
  });
}
