import { useQuery } from "@tanstack/react-query";

import { listNextSteps } from "../api/listNextSteps";

export function useNextSteps(asOf?: string) {
  return useQuery({
    queryKey: ["next-steps", asOf ?? "today"],
    queryFn: () => listNextSteps(asOf),
  });
}
