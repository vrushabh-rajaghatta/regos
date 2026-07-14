import { useQuery } from "@tanstack/react-query";

import { listSubmissions } from "../api/listSubmissions";

export function useSubmissions(applicationId: string) {
  return useQuery({
    queryKey: ["applications", applicationId, "submissions"],
    queryFn: () => listSubmissions(applicationId),
    enabled: !!applicationId,
  });
}
