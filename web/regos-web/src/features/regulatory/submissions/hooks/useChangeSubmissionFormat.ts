import { useMutation, useQueryClient } from "@tanstack/react-query";

import { changeSubmissionFormat } from "../api/changeSubmissionFormat";

export function useChangeSubmissionFormat(submissionId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (format: string) =>
      changeSubmissionFormat(submissionId, { format }),

    onSuccess: () => {
      // The submission itself carries the format, and the workspace header
      // reads the same query — one invalidation updates both.
      queryClient.invalidateQueries({
        queryKey: ["submissions", submissionId],
      });
    },
  });
}
