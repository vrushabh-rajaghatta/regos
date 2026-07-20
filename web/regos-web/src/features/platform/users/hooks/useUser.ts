import { useQuery } from "@tanstack/react-query";

import { getUser, UserNotFoundError } from "../api/getUser";

export function useUser(userId: string) {
  return useQuery({
    queryKey: ["users", "detail", userId],
    queryFn: () => getUser(userId),
    // A 404 is a definitive answer, not a transient failure worth retrying.
    retry: (failureCount, error) =>
      !(error instanceof UserNotFoundError) && failureCount < 2,
  });
}
