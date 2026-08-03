import { useQuery } from "@tanstack/react-query";

import { listApplicationStudies } from "../api/listApplicationStudies";

export function useApplicationStudies(applicationId: string) {
  return useQuery({
    queryKey: ["applications", applicationId, "studies"],
    queryFn: () => listApplicationStudies(applicationId),
  });
}
