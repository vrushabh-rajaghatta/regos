import { useQuery } from "@tanstack/react-query";

import { getCurrentUser } from "../api/getCurrentUser";

/**
 * Who the API says we are — and, because the session lives in cookies this code
 * cannot read, the only way to find out. There is no local token to inspect;
 * "am I signed in" is a question only the server can answer.
 */
export function useCurrentUser() {
  return useQuery({
    queryKey: ["currentUser"],
    queryFn: getCurrentUser,

    // apiFetch has already tried to refresh before this rejects, so a failure
    // means the session is genuinely over. Retrying produces more 401s.
    retry: false,

    // Signing out or being deactivated should be noticed reasonably promptly,
    // but not by asking on every render.
    staleTime: 60_000,
  });
}
