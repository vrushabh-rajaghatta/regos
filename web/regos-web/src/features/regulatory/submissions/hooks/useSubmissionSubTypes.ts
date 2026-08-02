import { useQuery } from "@tanstack/react-query";

import { listSubmissionSubTypes } from "../api/listSubmissionSubTypes";

export function useSubmissionSubTypes(authorityId: string) {
  return useQuery({
    queryKey: ["submission-sub-types", authorityId],
    queryFn: () => listSubmissionSubTypes(authorityId),
    enabled: !!authorityId,
  });
}
