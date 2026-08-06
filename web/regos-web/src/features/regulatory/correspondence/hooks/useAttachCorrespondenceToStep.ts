import { useMutation, useQueryClient } from "@tanstack/react-query";

import { attachCorrespondenceToStep } from "../api/attachCorrespondenceToStep";

export function useAttachCorrespondenceToStep(correspondenceId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (processStepId: string | null) =>
      attachCorrespondenceToStep(correspondenceId, processStepId),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["correspondence", "detail", correspondenceId],
      });
      queryClient.invalidateQueries({ queryKey: ["plans"] });
    },
  });
}
