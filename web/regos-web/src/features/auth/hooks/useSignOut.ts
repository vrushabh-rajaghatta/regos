import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";

import { clearAccessToken } from "@/shared/auth/accessToken";

/**
 * Signing out is entirely a client-side act today: the token stays valid until
 * it expires, and nothing tells the server. That is a real limitation of
 * stateless tokens with no revocation, and the reason access tokens are kept to
 * fifteen minutes. Revocation belongs with refresh tokens (AUTH-006).
 */
export function useSignOut() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  return () => {
    clearAccessToken();

    // Drop every cached response with it, so the next person to sign in on
    // this browser never sees the previous one's data.
    queryClient.clear();

    navigate("/login", { replace: true });
  };
}
