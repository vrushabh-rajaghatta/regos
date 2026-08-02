import { useQuery } from "@tanstack/react-query";

import { getSubmissionChanges } from "../api/getSubmissionChanges";

export function useSubmissionChanges(submissionId: string) {
  return useQuery({
    queryKey: ["submissions", submissionId, "changes"],
    queryFn: () => getSubmissionChanges(submissionId),
    enabled: !!submissionId,
  });
}
