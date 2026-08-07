import { useMutation, useQueryClient } from "@tanstack/react-query";

import { attachSubmissionToStep } from "../api/attachSubmissionToStep";

export function useAttachSubmissionToStep(submissionId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (processStepId: string | null) =>
      attachSubmissionToStep(submissionId, processStepId),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["submissions", submissionId] });
      queryClient.invalidateQueries({ queryKey: ["plans"] });
    },
  });
}
