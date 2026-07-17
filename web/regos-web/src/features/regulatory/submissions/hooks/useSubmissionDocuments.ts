import { useQuery } from "@tanstack/react-query";

import { listSubmissionDocuments } from "../api/listSubmissionDocuments";

export function useSubmissionDocuments(submissionId: string) {
  return useQuery({
    queryKey: ["submissions", submissionId, "documents"],
    queryFn: () => listSubmissionDocuments(submissionId),
    enabled: !!submissionId,
  });
}
