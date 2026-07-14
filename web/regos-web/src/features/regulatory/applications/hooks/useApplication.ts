import { useQuery } from "@tanstack/react-query";

import { getApplication } from "../api/getApplication";

export function useApplication(applicationId: string) {
  return useQuery({
    queryKey: ["applications", applicationId],
    queryFn: () => getApplication(applicationId),
    enabled: !!applicationId,
  });
}
