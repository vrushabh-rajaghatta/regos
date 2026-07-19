import { useMutation, useQueryClient } from "@tanstack/react-query";

import { publishSubmission } from "../api/publishSubmission";

export function usePublishSubmission(submissionId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => publishSubmission(submissionId),

    onSuccess: (outcome) => {
      // Only a successful publish changes server state worth refetching.
      if (outcome.published) {
        queryClient.invalidateQueries({
          queryKey: ["submissions", submissionId],
        });
        queryClient.invalidateQueries({
          queryKey: ["submissions", submissionId, "validation"],
        });
      }
    },
  });
}
