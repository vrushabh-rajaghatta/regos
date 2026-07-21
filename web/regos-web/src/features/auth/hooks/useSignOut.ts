import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";

import { logout } from "../api/logout";

/**
 * Signing out is now a server-side act: the refresh token is revoked, so it
 * cannot be used again even by someone who captured it.
 *
 * The access token is not revoked and cannot be — it is a signed statement, not
 * a database row. Clearing the cookie stops this browser from sending it, but
 * anyone who had extracted it keeps it until it expires. That is the reason
 * access tokens last fifteen minutes.
 */
export function useSignOut() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: logout,

    onSettled: () => {
      // Drop every cached response, so the next person to sign in on this
      // browser never sees the previous one's data.
      queryClient.clear();

      navigate("/login", { replace: true });
    },
  });
}
