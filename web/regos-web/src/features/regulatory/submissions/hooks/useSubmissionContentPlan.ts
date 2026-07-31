import { useQuery } from "@tanstack/react-query";

import { getSubmissionContentPlan } from "../api/getSubmissionContentPlan";

export function useSubmissionContentPlan(submissionId: string) {
  return useQuery({
    queryKey: ["submissions", submissionId, "content-plan"],
    queryFn: () => getSubmissionContentPlan(submissionId),
    enabled: !!submissionId,
  });
}
