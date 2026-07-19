import { useQuery } from "@tanstack/react-query";

import { validateSubmission } from "../api/validateSubmission";

export function useSubmissionValidation(submissionId: string) {
  return useQuery({
    queryKey: ["submissions", submissionId, "validation"],
    queryFn: () => validateSubmission(submissionId),
    enabled: !!submissionId,
  });
}
