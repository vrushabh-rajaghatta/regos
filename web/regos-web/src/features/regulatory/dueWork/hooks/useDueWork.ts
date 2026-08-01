import { useQuery } from "@tanstack/react-query";

import { listDueWork } from "../api/listDueWork";

export function useDueWork(mine: boolean, dueOnOrBefore?: string) {
  return useQuery({
    queryKey: ["due-work", mine, dueOnOrBefore ?? null],
    queryFn: () => listDueWork(mine, dueOnOrBefore),
  });
}
