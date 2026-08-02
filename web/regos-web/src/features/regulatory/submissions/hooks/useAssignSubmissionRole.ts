import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  assignSubmissionRole,
  type AssignSubmissionRoleRequest,
} from "../api/assignSubmissionRole";

export function useAssignSubmissionRole(submissionId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: AssignSubmissionRoleRequest) =>
      assignSubmissionRole(submissionId, request),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["submissions", submissionId, "roles"],
      });
    },
  });
}
