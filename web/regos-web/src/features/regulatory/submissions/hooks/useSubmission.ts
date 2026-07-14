import { useQuery } from "@tanstack/react-query";

import { getSubmission } from "../api/getSubmission";

export function useSubmission(submissionId: string) {
  return useQuery({
    queryKey: ["submissions", submissionId],
    queryFn: () => getSubmission(submissionId),
    enabled: !!submissionId,
  });
}
