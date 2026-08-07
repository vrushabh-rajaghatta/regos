import { useQuery } from "@tanstack/react-query";

import { getPlanImpact } from "../api/getPlanImpact";

export function usePlanImpact(planId: string | undefined, asOf?: string) {
  return useQuery({
    queryKey: ["plans", planId, "impact", asOf ?? "today"],
    queryFn: () => getPlanImpact(planId!, asOf),
    enabled: Boolean(planId),
  });
}
