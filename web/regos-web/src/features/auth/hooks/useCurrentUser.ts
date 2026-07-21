import { useQuery } from "@tanstack/react-query";

import { getAccessToken } from "@/shared/auth/accessToken";

import { getCurrentUser } from "../api/getCurrentUser";

export function useCurrentUser() {
  return useQuery({
    queryKey: ["currentUser"],
    queryFn: getCurrentUser,

    // No point asking who we are without a token; the request would 401 and
    // clear a session that was never there.
    enabled: Boolean(getAccessToken()),

    // A 401 means the token is finished, and apiFetch has already discarded
    // it. Retrying only produces more 401s.
    retry: false,
  });
}
