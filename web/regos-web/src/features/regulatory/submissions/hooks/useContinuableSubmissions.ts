import { useQuery } from "@tanstack/react-query";

import { listContinuableSubmissions } from "../api/listContinuableSubmissions";

export function useContinuableSubmissions(applicationId: string) {
  return useQuery({
    queryKey: ["applications", applicationId, "continuable-submissions"],
    queryFn: () => listContinuableSubmissions(applicationId),
    enabled: !!applicationId,
  });
}
