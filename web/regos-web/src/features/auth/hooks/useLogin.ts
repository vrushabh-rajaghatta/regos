import { useMutation, useQueryClient } from "@tanstack/react-query";

import { setAccessToken } from "@/shared/auth/accessToken";

import { login } from "../api/login";

export function useLogin() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: login,

    onSuccess: (result) => {
      setAccessToken(result.accessToken);

      // Everything cached was fetched as somebody else, or as nobody. Signing
      // in changes which organization the API answers for, so the previous
      // session's data must not survive into this one.
      queryClient.clear();
    },
  });
}
