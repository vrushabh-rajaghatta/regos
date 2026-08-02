import { useMutation, useQueryClient } from "@tanstack/react-query";

import { removeSubmissionRole } from "../api/removeSubmissionRole";

export function useRemoveSubmissionRole(submissionId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (roleId: string) => removeSubmissionRole(submissionId, roleId),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["submissions", submissionId, "roles"],
      });
    },
  });
}
