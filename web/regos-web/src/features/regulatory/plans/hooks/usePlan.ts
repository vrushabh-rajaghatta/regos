import { useQuery } from "@tanstack/react-query";

import { getPlan } from "../api/getPlan";

export function usePlan(id: string | undefined) {
  return useQuery({
    queryKey: ["plans", id],
    queryFn: () => getPlan(id!),
    enabled: Boolean(id),
  });
}
