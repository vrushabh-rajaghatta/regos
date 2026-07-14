import { useQuery } from "@tanstack/react-query";

import { listSubmissionTypes } from "../api/listSubmissionTypes";

export function useSubmissionTypes(authorityId: string) {
  return useQuery({
    queryKey: ["submission-types", authorityId],
    queryFn: () => listSubmissionTypes(authorityId),
    enabled: !!authorityId,
  });
}
