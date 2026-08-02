import { useQuery } from "@tanstack/react-query";

import { listSubmissionRoles } from "../api/listSubmissionRoles";

export function useSubmissionRoles(submissionId: string) {
  return useQuery({
    queryKey: ["submissions", submissionId, "roles"],
    queryFn: () => listSubmissionRoles(submissionId),
  });
}
