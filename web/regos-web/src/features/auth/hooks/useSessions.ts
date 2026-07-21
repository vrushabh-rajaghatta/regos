import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  getSessions,
  revokeOtherSessions,
  revokeSession,
} from "../api/sessions";

const SESSIONS = ["sessions"];

export function useSessions() {
  return useQuery({ queryKey: SESSIONS, queryFn: getSessions });
}

/**
 * Deliberately no `onSuccess` here, unlike its sibling below.
 *
 * Ending your *own* current session clears the cookies, so refetching the list
 * immediately afterwards is a guaranteed 401 — and awaiting that failed
 * invalidation pre-empts the caller's own success handler, which is how the
 * sign-out navigation silently stopped happening. Only the caller knows which
 * session it ended, so only the caller can decide whether refetching makes
 * sense.
 */
export function useRevokeSession() {
  return useMutation({ mutationFn: revokeSession });
}

export function useRevokeOtherSessions() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: revokeOtherSessions,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: SESSIONS }),
  });
}
