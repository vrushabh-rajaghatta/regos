import { useMutation, useQueryClient } from "@tanstack/react-query";

import { login } from "../api/login";

export function useLogin() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: login,

    onSuccess: async () => {
      // Everything cached was fetched as somebody else, or as nobody. Signing
      // in changes which organization the API answers for, so the previous
      // session's data must not survive into this one.
      queryClient.clear();

      // Resolve who we now are before the router lets anything render, so a
      // protected route does not briefly decide we are still signed out.
      await queryClient.refetchQueries({ queryKey: ["currentUser"] });
    },
  });
}
