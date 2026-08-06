import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  changePlanStatus,
  type ChangePlanStatusRequest,
} from "../api/changePlanStatus";

export function useChangePlanStatus(planId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ChangePlanStatusRequest) =>
      changePlanStatus(planId, request),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["plans", planId] });
      queryClient.invalidateQueries({ queryKey: ["objectives"] });
      queryClient.invalidateQueries({ queryKey: ["next-steps"] });
    },
  });
}
