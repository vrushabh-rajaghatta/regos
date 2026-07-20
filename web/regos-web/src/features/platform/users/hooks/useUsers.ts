import { keepPreviousData, useQuery } from "@tanstack/react-query";

import { listUsers, type ListUsersParams } from "../api/listUsers";

export function useUsers(params: ListUsersParams) {
  return useQuery({
    queryKey: ["users", params],
    queryFn: () => listUsers(params),
    // Keep the previous page visible while the next one loads, so paging and
    // searching do not flash the loading state on every keystroke.
    placeholderData: keepPreviousData,
  });
}
