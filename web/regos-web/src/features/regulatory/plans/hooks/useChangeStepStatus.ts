import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  changeStepStatus,
  type ChangeStepStatusRequest,
} from "../api/changeStepStatus";

export function useChangeStepStatus(planId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (variables: ChangeStepStatusRequest & { stepId: string }) =>
      changeStepStatus(planId, variables.stepId, variables),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["plans", planId] });
      queryClient.invalidateQueries({ queryKey: ["next-steps"] });
    },
  });
}
